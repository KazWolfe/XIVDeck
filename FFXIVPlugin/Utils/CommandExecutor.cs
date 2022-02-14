

using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.helpers;
using Lumina.Excel.GeneratedSheets;

using Action = Lumina.Excel.GeneratedSheets.Action;

namespace FFXIVPlugin.Utils {
    public class CommandExecutor {
        public static void Execute(HotbarSlotType type, uint actionId) {
            var plugin = XIVDeckPlugin.Instance;
            
            switch (type) {
                case HotbarSlotType.Emote:
                    var emoteData = Injections.DataManager.Excel.GetSheet<Emote>().GetRow(actionId);
                    var textCommand = emoteData.TextCommand.Value.Command;
                    PluginLog.Information($"Would call: {textCommand.ToString()}");
                    plugin.XivCommon.Functions.Chat.SendMessage(textCommand.ToString());
                    break;
                case HotbarSlotType.Action:
                    var actionData = Injections.DataManager.Excel.GetSheet<Action>().GetRow(actionId);
                    PluginLog.Information($"Would call: /ac \"{actionData.Name}\"");
                    plugin.XivCommon.Functions.Chat.SendMessage($"/ac \"{actionData.Name}\"");
                    break;
                case HotbarSlotType.Empty:
                    return;
                default:
                    throw new NotImplementedException($"I don't know how to handle an action of type {type}");
            }
        }
    }
}