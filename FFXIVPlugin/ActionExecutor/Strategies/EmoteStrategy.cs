using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils.Game;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Contract;
using EmotePayload = XIVDeck.FFXIVPlugin.ActionExecutor.Payloads.EmotePayload;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Emote)]
public class EmoteStrategy(
    IDataManager dataManager,
    ILogger pluginLog,
    IFramework framework,
    IGameConfig gameConfig,
    IUnlockState unlockState,
    IClientState clientState,
    ActionAppearanceResolver appearanceResolver) : UnlockableActionStrategy<Emote>(dataManager, framework, pluginLog, appearanceResolver) {
    protected override ActionEntry BuildActionEntry(Emote emote) {
        return new ActionEntry {
            Id = (int)emote.RowId,
            Name = emote.Name.ToString(),
            Category = emote.EmoteCategory.ValueNullable?.Name.ToString(),
            Type = HotbarSlotType.Emote,
            SortOrder = emote.Order
        };
    }

    // Emotes have special unlock/validation logic we need to respect.
    protected override unsafe bool IsUnlocked(Emote emote) {
        if (!clientState.IsLoggedIn) return false;

        if (emote.EmoteCategory.RowId == 0 || emote.Order == 0) return false;

        switch (emote.RowId) {
            case 55 when PlayerState.Instance()->GrandCompany != 1: // Maelstrom
            case 56 when PlayerState.Instance()->GrandCompany != 2: // Twin Adders
            case 57 when PlayerState.Instance()->GrandCompany != 3: // Immortal Flames
                return false;
        }

        return emote.UnlockLink == 0 || unlockState.IsEmoteUnlocked(emote);
    }

    protected override string GetLockedMessage(Emote emote) =>
        string.Format(UIStrings.EmoteStrategy_EmoteLockedError, emote.Name);

    protected override void ExecuteOnFramework(Emote emote, ActionPayload? payload) {
        bool? logMode = (payload as EmotePayload)?.LogMode switch {
            EmoteLogMode.Always => true,
            EmoteLogMode.Never => false,
            _ => null
        };

        using var _ = logMode != null ? gameConfig.UiConfig.TemporarySet("EmoteTextType", logMode.Value) : null;
        HotbarManager.ExecuteHotbarAction(HotbarSlotType.Emote, emote.RowId);
    }

    public override Type GetPayloadType() => typeof(EmotePayload);
}
