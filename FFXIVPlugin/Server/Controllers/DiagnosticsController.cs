using System.Collections.Generic;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/diagnostics")]
public class DiagnosticsController : WebApiController {

    [Route(HttpVerbs.Get, "/")]
    public Dictionary<string, object?> GetDiagnosticsReport() {
        return new Dictionary<string, object?> {
            ["Status"] = "online",
            ["Version"] = VersionUtils.GetCurrentMajMinBuild(),
            ["ApiKey"] = AuthHelper.Instance.Secret,
            ["Configuration"] = XIVDeckPlugin.Instance.Configuration
        };
    }

    [Route(HttpVerbs.Get, "/hello")]
    [Route(HttpVerbs.Get, "/hello/{num?}")]
    public string HelloWorld(int? num) {
        return $"hello world! #{num}";
    }
}


