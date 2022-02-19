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
    public class MarkerStrategy : FixedCommandStrategy<Marker> {
        protected override string GetNameForAction(Marker action) {
            return action.Name.RawString;
        }

        protected override HotbarSlotType GetHotbarSlotType() {
            return HotbarSlotType.Marker;
        }

        protected override int GetIconForAction(Marker action) {
            return action.Icon;
        }

        protected override string GetCommandToCallAction(Marker action) {
            return $"/enemysign \"{action.Name.RawString}\"";
        }
    }
}