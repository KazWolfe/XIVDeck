using System.Collections.Generic;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;

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
            IconId = this.GetIconId(actionId)
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
            throw new IllegalGameStateException(UIStrings.MacroStrategy_MacroEmptyError);
        }

        PluginLog.Debug($"Would execute macro number {macroNumber}");
        TickScheduler.Schedule(delegate {
            RaptureShellModule.Instance->ExecuteMacro(macro);
        });
    }

    public unsafe int GetIconId(uint item) {
        if (XIVDeckPlugin.Instance.Configuration.UseMIconIcons) {
            return this.GetAdjustedIconId(item);
        }
        
        var macro = GetMacro((item / 100 > 0), ((int) item % 100));
        return (int) macro->IconId;
    }

    private int GetAdjustedIconId(uint item) {
        var macroPage = item / 100;
        var macroId = item % 100;

        return TickScheduler.RunOnNextFrame(() => {
            // It's terrifying that the easiest way to get macro icon information is to just create a virtual
            // hotbar slot, but it's also the easiest and most effective.
            var slot = new HotBarSlot();
            slot.Set(HotbarSlotType.Macro, (macroPage << 8) + macroId);
            slot.LoadIconFromSlotB();

            return slot.Icon;
        }).Result;
    }
}