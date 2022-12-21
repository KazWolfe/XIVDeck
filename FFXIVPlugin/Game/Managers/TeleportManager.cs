using System;
using System.Linq;
using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;

namespace XIVDeck.FFXIVPlugin.Game.Managers;

public static class TeleportManager {
    public static bool Teleport(uint aetheryteId, byte subId) {
        var aetheryte = GetAetheryte(aetheryteId, subId);

        if (aetheryte is not null) {
            PluginLog.Debug($"Found requested aetheryte ({aetheryteId}.{subId}), teleporting...");
            return Teleport(aetheryte);
        }

        var luminaAetheryte = Injections.DataManager.GetExcelSheet<Aetheryte>()!.GetRow(aetheryteId);

        if (luminaAetheryte is null) 
            throw new ArgumentOutOfRangeException(nameof(aetheryteId), @$"No aetheryte with ID {aetheryteId} exists.");

        if (subId > 0)
            throw new IllegalGameStateException("The requested housing aetheryte is not available or does not exist.");

        throw new IllegalGameStateException($"The aetheryte {luminaAetheryte.PlaceName.Value?.Name} is not unlocked!");
    }

    public static unsafe bool Teleport(AetheryteEntry destination) {
        var localPlayer = Injections.ClientState.LocalPlayer;

        if (localPlayer == null)
            throw new PlayerNotLoggedInException();

        var actionStatus = ActionManager.Instance()->GetActionStatus(ActionType.Spell, 5);
        if (actionStatus != 0) {
            PluginLog.Warning($"Error code {actionStatus} while attempting to teleport.");
            throw new ActionLockedException("Cannot Teleport at this time!");
        }

        // housing sanity check
        if (IsHousingAetheryte(destination)) {
            if (localPlayer.CurrentWorld.Id != localPlayer.HomeWorld.Id) {
                var friendlyName = GetAetheryteName(destination);
                throw new IllegalGameStateException($"Cannot teleport to {friendlyName} while not on home world!");
            }
        }

        return Telepo.Instance()->Teleport(destination.AetheryteId, destination.SubIndex);
    }

    public static string GetAetheryteName(AetheryteEntry entry) {
        if (entry.IsAppartment) return AddonTextLoc.GetStringFromRowNumber(8518, "Apartment");
        if (entry.IsSharedHouse) return $"Shared Estate {entry.SubIndex}";

        return entry.AetheryteData.GameData?.PlaceName.Value?.Name ?? "Unknown Aetheryte";
    }

    public static bool IsHousingAetheryte(AetheryteEntry entry) {
        if (entry.Plot != 0 || entry.Ward != 0 || entry.SubIndex != 0) return true;

        // check for "Estate Hall ..." teleport names
        return entry.AetheryteData.GameData?.PlaceName.Row is 1145 or 1160;
    }

    public static AetheryteEntry? GetAetheryte(uint aetheryteId, byte subId = 0) {
        return Injections.AetheryteList.FirstOrDefault(entry =>
            entry.AetheryteId == aetheryteId && entry.SubIndex == subId);
    }
}