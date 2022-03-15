using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class InstrumentStrategy : IStrategy {
        private static readonly ExcelSheet<Perform> PerformSheet = Injections.DataManager.Excel.GetSheet<Perform>()!;
        
        public Perform GetInstrumentById(uint id) {
            return PerformSheet.GetRow(id);
        }

        public unsafe bool IsPerformUnlocked() {
            // APPARENTLY unlock 255 is performance?!
            return XIVDeckPlugin.Instance.XivCommon.Functions.Journal.IsQuestCompleted(68555) &&
                   UIState.Instance()->Hotbar.IsActionUnlocked(255);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            if (!this.IsPerformUnlocked()) return null;
            
            return PerformSheet.Where(i => i.RowId > 0).Select(instrument => new ExecutableAction {
                ActionId = (int) instrument.RowId, 
                ActionName = instrument.Instrument.RawString, 
                HotbarSlotType = HotbarSlotType.PerformanceInstrument
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            // intentionally not checking for Bard here; the game will take care of that for us (and display a better
            // error than we normally can). It's legal for a perform to be on a non-Bard hotbar, so I'm not concerned
            // about this.
            
            if (!this.IsPerformUnlocked()) {
                throw new InvalidOperationException("Performance mode hasn't yet been unlocked.");
            }
            
            if (Injections.Condition[ConditionFlag.Performing]) {
                throw new InvalidOperationException("Cannot switch instruments while actively in Perform mode.");
            }

            var instrument = this.GetInstrumentById(actionId);

            if (instrument == null) {
                throw new ArgumentOutOfRangeException(nameof(actionId), $"No instrument with ID {actionId} exists.");
            }
            
            TickScheduler.Schedule(delegate {
                XIVDeckPlugin.Instance.SigHelper.ExecuteHotbarAction(HotbarSlotType.PerformanceInstrument, actionId);
            });
        }

        public int GetIconId(uint actionId) {
            Perform instrument = this.GetInstrumentById(actionId);
            return instrument.Order;
        }
    }
}