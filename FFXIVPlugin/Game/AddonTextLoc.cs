using System;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using XIVDeck.FFXIVPlugin.IoC;

// ReSharper disable InconsistentNaming - resource file

namespace XIVDeck.FFXIVPlugin.Game;

[Service(ServiceFlags.Singleton)]
public class AddonTextLoc(IDataManager dataManager) {
    private readonly ExcelSheet<Addon> _addonTextSheet = dataManager.GetExcelSheet<Addon>();

    private string GetStringFromRowNumber(int rowId, string? fallback = null) {
        var row = this._addonTextSheet.GetRowOrDefault((uint) rowId);

        if (row == null)
            return fallback ?? throw new ArgumentOutOfRangeException(nameof(rowId), @$"Couldn't find Addon text row {rowId}");

        return row.Value.Text.ToDalamudString().ToString();
    }

    public string JobCategory_Tank => this.GetStringFromRowNumber(1082, "Tank");
    public string JobCategory_Healer => this.GetStringFromRowNumber(1083, "Healer");
    public string JobCategory_MeleeDPS => this.GetStringFromRowNumber(1084, "Melee DPS");
    public string JobCategory_RangedDPS => this.GetStringFromRowNumber(1085, "Physical Ranged DPS");
    public string JobCategory_CasterDPS => this.GetStringFromRowNumber(1086, "Magical Ranged DPS");
    public string JobCategory_DoH => this.GetStringFromRowNumber(802, "Disciples of the Hand");
    public string JobCategory_DoL => this.GetStringFromRowNumber(803, "Disciples of the Land");
}
