using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class EmoteStrategy : IStrategy {
        private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        
        public Emote GetEmoteById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Emote>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache.Refresh();

            return GameStateCache.UnlockedEmoteKeys!.Select(emote => new ExecutableAction() {
                ActionId = (int) emote.RowId, 
                ActionName = emote.Name.RawString, 
                HotbarSlotType = HotbarSlotType.Emote
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Emote emote = this.GetEmoteById(actionId);
            TextCommand textCommand = emote.TextCommand.Value;

            if (textCommand == null) {
                throw new KeyNotFoundException($"The emote \"{emote.Name.RawString}\" does not have an associated text command");
            }

            if (!GameStateCache.UnlockedEmoteKeys!.Contains(emote)) {
                throw new InvalidOperationException($"The emote \"{emote.Name.RawString}\" isn't unlocked and therefore can't be used.");
            }

            PluginLog.Debug($"Would execute command: {textCommand.Command.RawString}");
            XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(textCommand.Command.RawString);
        }

        public int GetIconId(uint item) {
            Emote emote = this.GetEmoteById(item);
            return emote.Icon;
        }
    }
}