using System;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class MacroStrategy : IStrategy {
        private static unsafe RaptureMacroModule.Macro* GetMacro(bool shared, int id) {
            var macroPage = shared ? &RaptureMacroModule.Instance->Shared : &RaptureMacroModule.Instance->Individual;
            return (*macroPage)[id];
        }
        
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
            
            RaptureMacroModule.Macro* macro = GetMacro(isSharedMacro, macroNumber);

            // Safety check to make sure we aren't triggering an empty macro
            if (RaptureMacroModule.Instance->GetLineCount(macro) == 0) {
                throw new ArgumentException("The specified macro is empty and cannot be used");
            }

            PluginLog.Debug($"Would execute macro number {macroNumber}");
            TickScheduler.Schedule(delegate {
                RaptureShellModule.Instance->ExecuteMacro(macro);
            });
        }

        public unsafe int GetIconId(uint item) {
            // todo: figure out how to get /micon, if set
            var macro = GetMacro((item / 100 > 0), ((int) item % 100));

            return (int) macro->IconId;
        }
    }
}