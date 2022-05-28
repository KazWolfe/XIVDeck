using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/command")]
public class CommandController : WebApiController {
    
    [Route(HttpVerbs.Post, "/")]
    public void ExecuteCommand([JsonData] SerializableTextCommand command) {
        if (!Injections.ClientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();

        if (command.Command is null or "" or "/")
            throw HttpException.BadRequest(UIStrings.CommandController_MissingCommandError);

        // only allow use of commands here for safety purposes (validating chat is hard)
        if (!command.Command.StartsWith("/") && command.SafeMode)
            throw HttpException.BadRequest(UIStrings.CommandController_NotCommandError);
            
        Injections.Framework.RunOnFrameworkThread(delegate {
            GameUtils.SendSanitizedChatMessage(command.Command, command.SafeMode);
        });
    }
}


