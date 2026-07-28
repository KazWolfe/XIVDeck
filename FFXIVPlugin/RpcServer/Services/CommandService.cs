using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using XIVDeck.FFXIVPlugin.Config;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Command")]
public class CommandService(IClientState clientState, IFramework framework) {

    public Task ExecuteCommand(TextCommandRequest commandRequest) {
        if (!clientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();

        if (commandRequest.Command is null or "" or "/")
            throw new MissingCommandException();

        GameUtils.SendDummyInput();

        return framework.RunOnFrameworkThread(delegate {
            ChatHelper.SendSanitizedChatMessage(commandRequest.Command);
        });
    }
}
