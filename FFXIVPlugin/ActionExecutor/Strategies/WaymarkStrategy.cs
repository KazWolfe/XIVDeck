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
    public class WaymarkStrategy : FixedCommandStrategy<FieldMarker> {
        protected override string GetNameForAction(FieldMarker action) {
            return action.Name.RawString;
        }

        protected override HotbarSlotType GetHotbarSlotType() {
            return HotbarSlotType.FieldMarker;
        }

        protected override int GetIconForAction(FieldMarker action) {
            return action.UiIcon;
        }

        protected override string GetCommandToCallAction(FieldMarker action) {
            return $"/waymark \"{action.Name.RawString}\"";
        }
    }
}