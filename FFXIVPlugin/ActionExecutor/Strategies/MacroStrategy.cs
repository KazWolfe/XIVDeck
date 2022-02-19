using System;
using System.Collections.Generic;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.helpers;
using FFXIVPlugin.Utils;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class MacroStrategy : IBaseStrategy {
        public List<ExecutableAction> GetAllowedItems() {
            // this will always return null as macros are a bit... weird. macro selection will take place completely
            // on the stream deck's side.
            return null;
        }
        
        public unsafe void Execute(uint actionId, dynamic _) {
            if (actionId > 199) {
                throw new ArgumentOutOfRangeException(nameof(actionId), "Action ID must be bound by 0 - 199");
            }

            bool isSharedMacro = actionId / 100 == 1;
            int macroNumber = (int) actionId % 100;

            // For safety, delegate the actual macro execution over to the Framework
            new TickScheduler(delegate {
                var macroPage = isSharedMacro
                    ? RaptureMacroModule.Instance->Shared
                    : RaptureMacroModule.Instance->Individual;
                
                PluginLog.Debug($"Would execute macro number {macroNumber} on page {macroPage}");
                // RaptureShellModule.Instance->ExecuteMacro(macroPage[macroNumber]);
            }, Injections.Framework);
        }

        public int GetIconId(uint item) {
            return 0;
        }
    }
}