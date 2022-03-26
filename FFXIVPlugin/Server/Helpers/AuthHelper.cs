using System;
using System.Security.Principal;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Authentication;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Helpers; 

public class AuthHelper {
    /**
     * This is a very simple authentication helper that will check a request against a per-invocation API key.
     *
     * This is *not* meant to be a secure form of authentication. It only really exists so that we can lock old versions
     * of the Stream Deck plugin out of the API (and prevent casual abuse).
     */
    
    private const string DevTestKey = "DevTest";
    public static readonly AuthHelper Instance = new();

    internal string Secret { get; }

    private AuthHelper() {
        this.Secret = CryptoUtils.GenerateToken(24);
    }

    public bool VerifyAuth(string? challenge) {
        if (string.IsNullOrWhiteSpace(challenge) || !challenge.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        var key = challenge.Replace("Bearer ", "");

        if (key == DevTestKey && XIVDeckPlugin.Instance.PluginInterface.IsDev) {
            return true;
        }
        
        return key == this.Secret;
    }
}

public class AuthModule : WebModuleBase {
    
    public AuthModule(string baseUrlPath) : base(baseUrlPath) { }
    
    public override bool IsFinalHandler => false;
    
    protected override Task OnRequestAsync(IHttpContext context) {
        
        // websocket is allowed to skip auth
        if (context.RequestedPath == "/ws") {
            return Task.CompletedTask;
        }
        
        var authHeader = context.Request.Headers[HttpHeaderNames.Authorization];

        if (!AuthHelper.Instance.VerifyAuth(authHeader)) 
            throw HttpException.Unauthorized("API key missing or invalid");
        
        ((IHttpContextImpl) context).User = new GenericPrincipal(new GenericIdentity(""), new[] {"api"});
        
        return Task.CompletedTask;
    }
}