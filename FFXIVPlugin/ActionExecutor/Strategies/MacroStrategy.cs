using System;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Macro)]
public class MacroStrategy : IActionStrategy {
    private static unsafe RaptureMacroModule.Macro* GetMacro(bool shared, int id) {
        return RaptureMacroModule.Instance()->GetMacro(shared ? 1u : 0u, (uint)id);
    }

    public unsafe ExecutableAction GetExecutableActionById(uint actionId) {
        var macro = GetMacro((actionId / 100 > 0), (int)actionId % 100);

        _ = TryGetMacroName(
            ref Unsafe.AsRef<RaptureMacroModule.Macro>(macro),
            (int)actionId % 100,
            (actionId / 100 > 0),
            out var macroName
        );

        // Macros are weird, inasmuch as they can't be null. Something will always exist, even if empty.
        return new ExecutableAction {
            ActionId = (int)actionId,
            ActionName = macroName,
            Category = null,
            HotbarSlotType = HotbarSlotType.Macro,
            IconId = this.GetIconId(actionId)
        };
    }

    public unsafe List<ExecutableAction> GetAllowedItems() {
        var items = new List<ExecutableAction>();

        if (Injections.ClientState.LocalPlayer != null)
            items.AddRange(this.GetValidMacrosFromCollection(RaptureMacroModule.Instance()->Individual, 0));

        items.AddRange(this.GetValidMacrosFromCollection(RaptureMacroModule.Instance()->Shared, 1));

        return items;
    }

    public unsafe void Execute(uint actionId, ActionPayload? _) {
        if (actionId > 199) {
            throw new ActionNotFoundException(HotbarSlotType.Macro, actionId);
        }

        var isSharedMacro = actionId / 100 == 1;
        var macroNumber = (int)actionId % 100;
        var macro = GetMacro(isSharedMacro, macroNumber);

        // Safety check to make sure we aren't triggering an empty macro
        if (RaptureMacroModule.Instance()->GetLineCount(macro) == 0) {
            throw new IllegalGameStateException(UIStrings.MacroStrategy_MacroEmptyError);
        }

        Injections.PluginLog.Debug($"Executing macro number {macroNumber}");
        Injections.Framework.RunOnFrameworkThread(delegate { RaptureShellModule.Instance()->ExecuteMacro(macro); });
    }

    public unsafe int GetIconId(uint item) {
        if (XIVDeckPlugin.Instance.Configuration.UseMIconIcons) {
            return this.GetAdjustedIconId(item);
        }

        var macro = GetMacro((item / 100 > 0), ((int)item % 100));
        return (int)macro->IconId;
    }

    private int GetAdjustedIconId(uint item) {
        var macroPage = item / 100;
        var macroId = item % 100;

        return Injections.Framework.RunOnFrameworkThread(() => {
            // It's terrifying that creating a virtual hotbar slot is probably the easiest way to get a macro icon ID,
            // but here we are.

            var slot = new HotbarSlot();
            slot.Set(HotbarSlotType.Macro, (macroPage << 8) + macroId);
            slot.LoadIconId();

            return (int)slot.IconId;
        }).Result;
    }

    private List<ExecutableAction> GetValidMacrosFromCollection(Span<RaptureMacroModule.Macro> span, int pageId) {
        var result = new List<ExecutableAction>();

        for (var i = 0; i < span.Length; i++) {
            ref var macro = ref span[i];

            var macroId = (100 * pageId) + i;
            var macroIconId = (int)macro.IconId;
            var wasMacroNamed = TryGetMacroName(ref macro, i, pageId == 1, out var macroName);

            if (!wasMacroNamed && macroIconId == 0) continue;

            result.Add(new ExecutableAction {
                ActionId = macroId,
                ActionName = macroName,
                HotbarSlotType = HotbarSlotType.Macro,
                IconId = macroIconId,  // intentionally not resolving the proper icon id here. it's very slow.
            });
        }

        return result;
    }

    /// <summary>
    /// Checks if the macro is named, and returns the name.
    /// </summary>
    /// <param name="macro">A pointer to the macro to check.</param>
    /// <param name="id">The ID of the macro for name generation purposes.</param>
    /// <param name="isShared">The share state of the macro for name generation purposes.</param>
    /// <param name="name">An out var containing the determined name of the macro.</param>
    /// <returns>Returns true if the macro was named, false if a generic name was used.</returns>
    private static bool TryGetMacroName(ref RaptureMacroModule.Macro macro, int id, bool isShared, out string name) {
        name = macro.Name.ToString();
        if (!name.IsNullOrEmpty()) return true;

        name = isShared ? $"Shared Macro {id}" : $"Individual Macro {id}";
        return false;
    }
}
