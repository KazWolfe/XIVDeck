using System.Collections.Generic;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.Macro)]
public class MacroStrategy : IActionStrategy {
    private static unsafe RaptureMacroModule.Macro* GetMacro(bool shared, int id) {
        var macroPage = shared ? &RaptureMacroModule.Instance->Shared : &RaptureMacroModule.Instance->Individual;
        return (*macroPage)[id];
    }

    public unsafe ExecutableAction GetExecutableActionById(uint actionId) {
        var macro = GetMacro((actionId / 100 > 0), ((int) actionId % 100));

        // Macros are weird, inasmuch as they can't be null. Something will always exist, even if empty.
        return new ExecutableAction {
            ActionId = (int) actionId,
            ActionName = macro->Name.ToString(),
            Category = null,
            HotbarSlotType = HotbarSlotType.Macro,
            IconId = (int) macro->IconId
        };
    }

    public List<ExecutableAction>? GetAllowedItems() {
        // this will always return null as macros are a bit... weird. macro selection will take place completely
        // on the stream deck's side.
        return null;
    }

    public unsafe void Execute(uint actionId, dynamic? _) {
        if (actionId > 199) {
            throw new ActionNotFoundException(HotbarSlotType.Macro, actionId);
        }

        var isSharedMacro = actionId / 100 == 1;
        var macroNumber = (int) actionId % 100;
        var macro = GetMacro(isSharedMacro, macroNumber);

        // Safety check to make sure we aren't triggering an empty macro
        if (RaptureMacroModule.Instance->GetLineCount(macro) == 0) {
            throw new IllegalGameStateException("The specified macro is empty and cannot be used.");
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