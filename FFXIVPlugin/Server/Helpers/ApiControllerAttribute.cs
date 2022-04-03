using System;
using System.Reflection;
using EmbedIO;
using EmbedIO.WebApi;

namespace XIVDeck.FFXIVPlugin.Server.Helpers;

[AttributeUsage(AttributeTargets.Class)]
public class ApiControllerAttribute : Attribute {
    public readonly string BaseUrl;

    public ApiControllerAttribute(string baseUrl) {
        this.BaseUrl = baseUrl;
    }
}

public static class ApiControllerWiring {
    public static void Autowire(IWebServer webServer) {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes()) {
            var controllerAttribute = type.GetCustomAttribute<ApiControllerAttribute>();

            if (controllerAttribute != null) {
                webServer.WithWebApi(controllerAttribute.BaseUrl, NewtonsoftJsonShim.Serialize, m => {
                    m.WithController(type);
                });
            }
        }
    }
}