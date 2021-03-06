using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
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
            HotbarSlotType = HotbarSlotType.Emote
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

    public void Execute(uint actionId, dynamic? _) {
        var emote = GetEmoteById(actionId);
            
        if (emote == null) {
            throw new ActionNotFoundException(HotbarSlotType.Emote, actionId);
        }
            
        var textCommand = emote.TextCommand.Value;

        if (textCommand == null) {
            throw new KeyNotFoundException(string.Format(UIStrings.EmoteStrategy_EmoteDoesntHaveCommandError, emote.Name));
        }

        if (!emote.IsUnlocked()) {
            throw new ActionLockedException(string.Format(UIStrings.EmoteStrategy_EmoteLockedError, emote.Name));
        }

        PluginLog.Debug($"Would execute command: {textCommand.Command}");
        Injections.Framework.RunOnFrameworkThread(delegate {
            GameUtils.SendSanitizedChatMessage(textCommand.Command);
        });
    }

    public int GetIconId(uint item) {
        return GetEmoteById(item)?.Icon ?? 0;

    }
}