using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using FFXIVPlugin.helpers;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class EmoteStrategy : IBaseStrategy {
        public Emote GetEmoteById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Emote>().GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
            gameStateCache.Refresh();

            return gameStateCache.UnlockedEmoteKeys.Select(emote => new ExecutableAction() {
                ActionId = (int) emote.RowId, 
                ActionName = emote.Name.RawString, 
                HotbarSlotType = HotbarSlotType.Emote
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Emote emote = GetEmoteById(actionId);
            TextCommand textCommand = emote.TextCommand.Value;

            if (textCommand == null) {
                throw new KeyNotFoundException($"The emote {emote} does not have an associated text command");
            }

            PluginLog.Debug($"Would execute command: {textCommand.Command.RawString}");
            // XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(textCommand.Command.RawString);
        }

        public int GetIconId(uint item) {
            Emote emote = GetEmoteById(item);
            return emote.Icon;
        }
    }
}