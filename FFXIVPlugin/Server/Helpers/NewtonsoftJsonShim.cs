using System;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace XIVDeck.FFXIVPlugin.Server.Helpers; 

public static class NewtonsoftJsonShim {
    private static readonly JsonSerializerSettings JSettings = new() {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    
    public static async Task Serialize(IHttpContext context, object? data) {
        Validate.NotNull(nameof(context), context).Response.ContentType = MimeType.Json;
        await using var text = context.OpenResponseText(new UTF8Encoding(false));
        await text.WriteAsync(JsonConvert.SerializeObject(data, JSettings)).ConfigureAwait(false);
    }

    public static async Task<TData> Deserialize<TData>(IHttpContext context) {
        Validate.NotNull(nameof(context), context);

        string body;
        using (var reader = context.OpenRequestText())
        {
            body = await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        try {
            return JsonConvert.DeserializeObject<TData>(body) ?? throw new FormatException();
        } catch (FormatException) {
            throw HttpException.BadRequest("Incorrect request data format.");
        }
    }
}