using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/command")]
public class CommandController : WebApiController {
    
    [Route(HttpVerbs.Post, "/")]
    public void ExecuteCommand([JsonData] SerializableTextCommand command) {
        if (!Injections.ClientState.IsLoggedIn)
            throw HttpException.Unauthorized("A player is not logged in to the game!");

        if (command.Command == null)
            throw HttpException.BadRequest("A command is required for execution");

        // only allow use of commands here for safety purposes (validating chat is hard)
        if (!command.Command.StartsWith("/") && command.SafeMode)
            throw HttpException.BadRequest("Commands must start with a slash.");
            
        TickScheduler.Schedule(delegate {
            ChatUtil.SendSanitizedChatMessage(command.Command, command.SafeMode);
        });
    }
}


