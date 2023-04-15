using System.Dynamic;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/diagnostics")]
public class DiagnosticsController : WebApiController {

    [Route(HttpVerbs.Get, "/")]
    public dynamic GetDiagnosticsReport() {
        dynamic report = new ExpandoObject();
        
        report.Status = "online";
        report.Version = VersionUtils.GetCurrentMajMinBuild();
        report.ApiKey = AuthHelper.Instance.Secret;
        report.Configuration = XIVDeckPlugin.Instance.Configuration;
        
        return report;
    }

    [Route(HttpVerbs.Get, "/hello")]
    [Route(HttpVerbs.Get, "/hello/{num?}")]
    public string HelloWorld(int? num) {
        return $"hello world! #{num}";
    }
}


