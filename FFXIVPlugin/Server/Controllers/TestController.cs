using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Server.Helpers;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/test")]
public class TestController : WebApiController {
    
    [Route(HttpVerbs.Get, "/")]
    [Route(HttpVerbs.Get, "/{num?}")]
    public string Base(int? num) {
        return $"hello world! #{num}";
    }
}


