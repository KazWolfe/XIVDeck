using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class MinionStrategy : IActionStrategy {
        private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;

        private static ExecutableAction GetExecutableAction(Companion minion) {
            return new ExecutableAction {
                ActionId = (int) minion.RowId,
                ActionName = minion.Singular.RawString,
                IconId = minion.Icon,
                HotbarSlotType = HotbarSlotType.Minion
            };
        }
        
        private static Companion? GetMinionById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Companion>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache.Refresh();
            return GameStateCache.UnlockedMinions!.Select(GetExecutableAction).ToList();
        }

        public ExecutableAction? GetExecutableActionById(uint actionId) {
            var action = GetMinionById(actionId);
            return action == null ? null : GetExecutableAction(action);
        }

        public void Execute(uint actionId, dynamic? _) {
            Companion? minion = GetMinionById(actionId);
            
            if (minion == null) {
                throw new ArgumentNullException(nameof(actionId), $"No minion with ID {actionId} exists.");
            }

            if (!GameStateCache.IsMinionUnlocked(actionId)) {
                throw new ActionLockedException($"The minion \"{minion.Singular.RawString}\" isn't unlocked and therefore can't be used.");
            }
            
            var command = $"/minion \"{minion.Singular.RawString}\"";
            
            PluginLog.Debug($"Would execute command: {command}");
            
            TickScheduler.Schedule(delegate {
                ChatUtil.SendSanitizedChatMessage(command);
            });
        }

        public int GetIconId(uint item) {
            return GetMinionById(item)?.Icon ?? 0;
        }
    }
}