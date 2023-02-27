using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Emote)]
public class EmoteStrategy : IActionStrategy {
    private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;

    private static ExecutableAction GetExecutableAction(Emote emote) {
        return new ExecutableAction {
            ActionId = (int) emote.RowId,
            ActionName = emote.Name.ToString(),
            IconId = emote.Icon,
            Category = emote.EmoteCategory.Value?.Name.ToString() ?? null,
            HotbarSlotType = HotbarSlotType.Emote,
            SortOrder = emote.Order
        };
    }

    private static Emote? GetEmoteById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Emote>()!.GetRow(id);
    }

    public ExecutableAction? GetExecutableActionById(uint slotId) {
        var emote = GetEmoteById(slotId);

        return emote == null ? null : GetExecutableAction(emote);
    }

    public List<ExecutableAction> GetAllowedItems() {
        GameStateCache.Refresh();

        return GameStateCache.UnlockedEmotes!.Select(GetExecutableAction).ToList();
    }

    public void Execute(uint actionId, ActionPayload? payload) {
        bool? logMode = null;
        if (payload is EmotePayload ep) {
            logMode = ep.LogMode switch {
                EmoteLogMode.Always => true,
                EmoteLogMode.Never => false,
                _ => null
            };
        }

        var emote = GetEmoteById(actionId);

        if (emote == null) {
            throw new ActionNotFoundException(HotbarSlotType.Emote, actionId);
        }

        var textCommand = emote.TextCommand.Value;

        if (textCommand == null) {
            throw new KeyNotFoundException(string.Format(UIStrings.EmoteStrategy_EmoteDoesntHaveCommandError,
                emote.Name));
        }

        if (!emote.IsUnlocked()) {
            throw new ActionLockedException(string.Format(UIStrings.EmoteStrategy_EmoteLockedError, emote.Name));
        }

        PluginLog.Debug($"Executing command: {textCommand.Command}");
        Injections.Framework.RunOnFrameworkThread(delegate {
            using var _ = logMode != null ? GameConfig.UiConfig.TemporarySet(ConfigOption.EmoteTextType, logMode.Value) : null;
            ChatHelper.GetInstance().SendSanitizedChatMessage(textCommand.Command);
        });
    }

    public int GetIconId(uint item) {
        return GetEmoteById(item)?.Icon ?? 0;
    }

    public Type GetPayloadType() {
        return typeof(EmotePayload);
    }
}