using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Excel;
using FFXIVPlugin.Base;
using FFXIVPlugin.helpers;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class GeneralActionStrategy : FixedCommandStrategy<GeneralAction> {
        protected override uint[] GetIllegalActionIDs() {
            // todo: find a way to hide advanced materia melding if not unlocked
            return new uint[]{ 11, 18, 23, 24 };
        }
        
        protected override string GetNameForAction(GeneralAction action) {
            return action.Name.RawString;
        }

        protected override HotbarSlotType GetHotbarSlotType() {
            return HotbarSlotType.GeneralAction;
        }

        protected override int GetIconForAction(GeneralAction action) {
            return action.Icon;
        }

        protected override string GetCommandToCallAction(GeneralAction action) {
            return $"/generalaction \"{action.Name.RawString}\"";
        }
    }
}