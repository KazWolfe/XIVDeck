using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Serilog;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Types;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Macro)]
public class MacroStrategy(ILogger pluginLog, IFramework framework,
    ActionAppearanceResolver appearanceResolver) : IActionStrategy {
    private static unsafe RaptureMacroModule.Macro* GetMacro(MacroId macroId) {
        return RaptureMacroModule.Instance()->GetMacro(macroId.Page, macroId.Index);
    }

    public unsafe ActionEntry? GetActionEntryById(uint actionId) {
        if (!MacroId.TryFromActionId(actionId, out var macroId)) return null;

        var macroName = GetMacroName(ref Unsafe.AsRef<RaptureMacroModule.Macro>(GetMacro(macroId)), macroId);

        // Macros are weird, inasmuch as they can't be null. Something will always exist, even if empty.
        return new ActionEntry {
            Id = (int)macroId.ToActionId(),
            Name = macroName,
            Category = null,
            Type = HotbarSlotType.Macro,
        };
    }

    // Don't include macros in the action list, since they're executed through a different system.
    public List<ActionEntry> GetSelectableActions() => [];

    public async Task Execute(uint actionId, ActionPayload? _) {
        var macroId = RequireMacroId(actionId);

        // Safety check to make sure we aren't triggering an empty macro
        if (!HasMacroLines(macroId)) {
            throw new IllegalGameStateException(UIStrings.MacroStrategy_MacroEmptyError);
        }

        pluginLog.Debug("Executing macro {MacroId}", macroId);
        await framework.RunOnFrameworkThread(() => ExecuteMacroUnsafe(macroId));
    }

    public Task<ActionAppearance> GetAppearance(uint actionId) {
        var macroId = RequireMacroId(actionId);

        return appearanceResolver.GetActionAppearance(HotbarSlotType.Macro, macroId.ToHotbarCommandId());
    }

    private static MacroId RequireMacroId(uint actionId) {
        if (!MacroId.TryFromActionId(actionId, out var macroId)) {
            throw new ActionNotFoundException(HotbarSlotType.Macro, actionId);
        }

        return macroId;
    }

    private static string GetMacroName(ref RaptureMacroModule.Macro macro, MacroId macroId) {
        var name = macro.Name.ToString();
        if (!name.IsNullOrEmpty()) return name;

        var fallback = macroId.Shared ? UIStrings.MacroStrategy_SharedMacroName : UIStrings.MacroStrategy_IndividualMacroName;
        return string.Format(fallback, macroId.Index);
    }

    private static unsafe bool HasMacroLines(MacroId macroId) {
        return RaptureMacroModule.Instance()->GetLineCount(GetMacro(macroId)) != 0;
    }

    private static unsafe void ExecuteMacroUnsafe(MacroId macroId) {
        RaptureShellModule.Instance()->ExecuteMacro(GetMacro(macroId));
    }
}
