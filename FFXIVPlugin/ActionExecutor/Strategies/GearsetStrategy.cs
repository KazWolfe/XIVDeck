using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public unsafe class GearsetStrategy : IStrategy {
        private static GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        private static RaptureGearsetModule* _gsModule = RaptureGearsetModule.Instance();
        
        public GameStateCache.Gearset GetGearsetBySlot(uint slot) {
            // we want to intentionally bypass the cache here as the cache is a bit messy and, well, not always up to
            // date. this can get called quite a bit, so we don't want to force updating everything each time (which
            // (invalidates the entire point of the cache).
            
            var gsEntry = _gsModule->Gearset[(int) slot - 1];

            if (gsEntry == null || !gsEntry->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists)) 
                throw new ArgumentException($"The gearset in slot {slot} could not be found");

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
            
            String command = $"/gearset change {gearset.Slot + 1}";
            
            PluginLog.Debug($"Would execute command: {command}");
            XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(command);
        }

        public int GetIconId(uint slot) {
            return 062800 + (int) this.GetGearsetBySlot(slot).ClassJob;
        }
    }
}