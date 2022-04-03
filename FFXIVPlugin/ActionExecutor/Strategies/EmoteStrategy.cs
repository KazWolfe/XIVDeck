using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Utils;

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
        Emote? emote = GetEmoteById(actionId);
            
        if (emote == null) {
            throw new ActionNotFoundException(HotbarSlotType.Emote, actionId);
        }
            
        TextCommand? textCommand = emote.TextCommand.Value;

        if (textCommand == null) {
            throw new KeyNotFoundException($"The emote \"{emote.Name}\" does not have an associated text command.");
        }

        if (!GameStateCache.IsEmoteUnlocked(emote.RowId)) {
            throw new ActionLockedException($"The emote \"{emote.Name}\" isn't unlocked and therefore can't be used.");
        }

        PluginLog.Debug($"Would execute command: {textCommand.Command}");
        TickScheduler.Schedule(delegate {
            ChatUtils.SendSanitizedChatMessage(textCommand.Command);
        });
    }

    public int GetIconId(uint item) {
        return GetEmoteById(item)?.Icon ?? 0;

    }
}