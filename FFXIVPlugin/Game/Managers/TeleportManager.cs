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
        if (IsHousingAetheryte(destination) && localPlayer.CurrentWorld.Id != localPlayer.HomeWorld.Id) {
            var friendlyName = GetAetheryteName(destination);
            throw new IllegalGameStateException($"Cannot teleport to {friendlyName} while not on home world!");
        }

        return Telepo.Instance()->Teleport(destination.AetheryteId, destination.SubIndex);
    }

    public static string GetAetheryteName(Aetheryte? aetheryte, byte subId = 0) {
        if (aetheryte == null) {
            return "Unknown Aetheryte";
        }
        
        return subId switch {
            128 => AddonTextLoc.GetStringFromRowNumber(8518, "Apartment"),
            > 0 => $"Unknown Shared Estate {subId}",
            _ => aetheryte.PlaceName.Value?.Name ?? "Unknown Aetheryte"
        };
    }

    public static string GetAetheryteName(AetheryteEntry entry) {
        if (entry.IsSharedHouse) {
            var regionName = entry.AetheryteData.GameData?.TerritoryName() ?? "???";
            return $"{regionName}, Ward {entry.Ward} Plot {entry.Plot}";
        }

        return GetAetheryteName(entry.AetheryteData.GameData, entry.SubIndex);
    }

    public static bool IsHousingAetheryte(AetheryteEntry entry) {
        if (entry.Plot != 0 || entry.Ward != 0 || entry.SubIndex != 0) return true;
        
        return IsHousingAetheryte(entry.AetheryteData.GameData, entry.SubIndex);
    }

    public static bool IsHousingAetheryte(Aetheryte? aetheryte, byte subId = 0) {
        if (aetheryte == null) return false;
        if (subId != 0) return true;

        // check for "Estate Hall ..." teleport names
        return aetheryte.PlaceName.Row is 1145 or 1160;
    }

    public static AetheryteEntry? GetAetheryte(uint aetheryteId, byte subId = 0) {
        return Injections.AetheryteList.FirstOrDefault(entry =>
            entry.AetheryteId == aetheryteId && entry.SubIndex == subId);
    }
}