using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class EmoteStrategy : IStrategy {
        private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        
        public Emote? GetEmoteById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Emote>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache.Refresh();

            return GameStateCache.UnlockedEmotes!.Select(emote => new ExecutableAction() {
                ActionId = (int) emote.RowId, 
                ActionName = emote.Name.RawString, 
                HotbarSlotType = HotbarSlotType.Emote
            }).ToList();
        }

        public void Execute(uint actionId, dynamic? _) {
            Emote? emote = this.GetEmoteById(actionId);
            
            if (emote == null) {
                throw new ArgumentNullException(nameof(actionId), $"No emote with ID {actionId} exists.");
            }
            
            TextCommand? textCommand = emote.TextCommand.Value;

            if (textCommand == null) {
                throw new KeyNotFoundException($"The emote \"{emote.Name.RawString}\" does not have an associated text command.");
            }

            if (!GameStateCache.IsEmoteUnlocked(emote.RowId)) {
                throw new InvalidOperationException($"The emote \"{emote.Name.RawString}\" isn't unlocked and therefore can't be used.");
            }

            PluginLog.Debug($"Would execute command: {textCommand.Command.RawString}");
            
            TickScheduler.Schedule(delegate {
                ChatUtil.SendSanitizedChatMessage(textCommand.Command.RawString);
            });
        }

        public int GetIconId(uint item) {
            return this.GetEmoteById(item)?.Icon ?? 0;

        }
    }
}