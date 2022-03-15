using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public unsafe class GearsetStrategy : IStrategy {
        private static GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        private static RaptureGearsetModule* _gsModule = RaptureGearsetModule.Instance();
        
        public GameStateCache.Gearset? GetGearsetBySlot(uint slot) {
            // we want to intentionally bypass the cache here as the cache is a bit messy and, well, not always up to
            // date. this can get called quite a bit, so we don't want to force updating everything each time (which
            // (invalidates the entire point of the cache).
            
            var gsEntry = _gsModule->Gearset[(int) slot - 1];

            if (gsEntry == null || !gsEntry->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists))
                return null;

            return new GameStateCache.Gearset {
                Slot = gsEntry->ID,
                ClassJob = gsEntry->ClassJob,
                Name = MemoryHelper.ReadString(new IntPtr(gsEntry->Name), 47)
            };
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            _gameStateCache.Refresh();
            
            return _gameStateCache.Gearsets!.Select(gearset => new ExecutableAction {
                ActionId = gearset.Slot, 
                ActionName = gearset.Name, 
                HotbarSlotType = HotbarSlotType.GearSet,
            }).ToList();
        }

        public void Execute(uint actionSlot, dynamic _) {
            var gearset =  this.GetGearsetBySlot(actionSlot);

            if (gearset == null)
                throw new ArgumentException($"No gearset exists in slot number {actionSlot}.");

            String command = $"/gearset change {gearset.Value.Slot + 1}";
            
            PluginLog.Debug($"Would execute command: {command}");
            TickScheduler.Schedule(delegate {
                ChatUtil.SendSanitizedChatMessage(command);
            });
        }

        public int GetIconId(uint slot) {
            var gearset = this.GetGearsetBySlot(slot);

            if (gearset == null) return 0;
            
            return 062800 + (int) gearset.Value.ClassJob;
        }
    }
}