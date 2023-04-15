using System;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;

// ReSharper disable InconsistentNaming - resource file

namespace XIVDeck.FFXIVPlugin.Game; 

/// <summary>
/// Class which provides easy access to translated localizations direct from game addons.
/// </summary>
public static class AddonTextLoc {
    private static readonly ExcelSheet<Addon>? AddonTextSheet = Injections.DataManager.GetExcelSheet<Addon>();

    public static string GetStringFromRowNumber(int rowId, string? fallback = null) {
        if (AddonTextSheet == null) 
            return fallback ?? throw new NullReferenceException("Failed to load Addon sheet from Lumina");

        var row = AddonTextSheet.GetRow((uint) rowId);

        if (row == null)
            return fallback ?? throw new ArgumentOutOfRangeException(nameof(rowId), @$"Couldn't find Addon text row {rowId}");

        return row.Text.ToDalamudString().ToString();
    }

    public static string JobCategory_Tank => GetStringFromRowNumber(1082, "Tank");
    public static string JobCategory_Healer => GetStringFromRowNumber(1083, "Healer");
    public static string JobCategory_MeleeDPS => GetStringFromRowNumber(1084, "Melee DPS");
    public static string JobCategory_RangedDPS => GetStringFromRowNumber(1085, "Physical Ranged DPS");
    public static string JobCategory_CasterDPS => GetStringFromRowNumber(1086, "Magical Ranged DPS");
    public static string JobCategory_DoH => GetStringFromRowNumber(802, "Disciples of the Hand");
    public static string JobCategory_DoL => GetStringFromRowNumber(803, "Disciples of the Land");
}