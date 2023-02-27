using System.Collections.Generic;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Macro)]
public class MacroStrategy : IActionStrategy {
    private static unsafe RaptureMacroModule.Macro* GetMacro(bool shared, int id) {
        var macroPage = shared ? &RaptureMacroModule.Instance->Shared : &RaptureMacroModule.Instance->Individual;
        return (*macroPage)[id];
    }

    public unsafe ExecutableAction GetExecutableActionById(uint actionId) {
        var macro = GetMacro((actionId / 100 > 0), (int) actionId % 100);

        // Macros are weird, inasmuch as they can't be null. Something will always exist, even if empty.
        return new ExecutableAction {
            ActionId = (int) actionId,
            ActionName = macro->Name.ToString(),
            Category = null,
            HotbarSlotType = HotbarSlotType.Macro,
            IconId = this.GetIconId(actionId)
        };
    }

    // Macros will always return null, as they're selected on the client side and we don't get a say.
    public List<ExecutableAction>? GetAllowedItems() => null;

    public unsafe void Execute(uint actionId, ActionPayload? _) {
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

        PluginLog.Debug($"Executing macro number {macroNumber}");
        Injections.Framework.RunOnFrameworkThread(delegate { RaptureShellModule.Instance->ExecuteMacro(macro); });
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

        return Injections.Framework.RunOnTick(() => {
            // It's terrifying that creating a virtual hotbar slot is probably the easiest way to get a macro icon ID,
            // but here we are.

            var slot = new HotBarSlot();
            slot.Set(HotbarSlotType.Macro, (macroPage << 8) + macroId);
            slot.LoadIconFromSlotB();

            return slot.Icon;
        }).Result;
    }
}