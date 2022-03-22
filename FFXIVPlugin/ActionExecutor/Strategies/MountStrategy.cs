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
    public class MountStrategy : IActionStrategy {
        private static GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;

        private static ExecutableAction GetExecutableAction(Mount mount) {
            return new ExecutableAction {
                ActionId = (int) mount.RowId,
                ActionName = mount.Singular.RawString,
                IconId = mount.Icon,
                HotbarSlotType = HotbarSlotType.Mount
            };
        }
        
        private static Mount? GetMountById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Mount>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            _gameStateCache.Refresh();

            return _gameStateCache.UnlockedMounts!.Select(GetExecutableAction).ToList();
        }

        public void Execute(uint actionId, dynamic? _) {
            Mount? mount = GetMountById(actionId);

            if (mount == null) {
                throw new ArgumentNullException(nameof(actionId), $"No mount with ID {actionId} exists.");
            }
            
            if (!_gameStateCache.IsMountUnlocked(actionId)) {
                throw new ActionLockedException($"The mount \"{mount.Singular.RawString}\" isn't unlocked and therefore can't be used.");
            }
            
            String command = $"/mount \"{mount.Singular.RawString}\"";
            
            PluginLog.Debug($"Would execute command: {command}");
            TickScheduler.Schedule(delegate {
                ChatUtil.SendSanitizedChatMessage(command);
            });
        }
        
        public ExecutableAction? GetExecutableActionById(uint actionId) {
            var action = GetMountById(actionId);
            return action == null ? null : GetExecutableAction(action);
        }

        public int GetIconId(uint item) {
            return GetMountById(item)?.Icon ?? 0;

        }
    }
}