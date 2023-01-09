using System;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Helpers; 

public static class NewtonsoftJsonShim {
    private static readonly JsonSerializerSettings JSettings = new();
    
    public static async Task Serialize(IHttpContext context, object? data) {
        Validate.NotNull(nameof(context), context).Response.ContentType = MimeType.Json;
        await using var text = context.OpenResponseText(new UTF8Encoding(false));
        await text.WriteAsync(JsonConvert.SerializeObject(data, JSettings)).ConfigureAwait(false);
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class NJsonData : Attribute, IRequestDataAttribute<WebApiController> {
    public async Task<object?> GetRequestDataAsync(WebApiController controller, Type type, string parameterName) {
        string body;
        using (var reader = controller.HttpContext.OpenRequestText()) {
            body = await reader.ReadToEndAsync().ConfigureAwait(false);
        }
        
        return JsonConvert.DeserializeObject(body, type);
    }
}