using System;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.Game;

[Service(ServiceFlags.Singleton)]
public class GameClassFactory(IDataManager dataManager, AddonTextLoc addonTextLoc) {
    public SerializableGameClass Create(int id) {
        var classJob = dataManager.GetExcelSheet<ClassJob>().GetRowOrDefault((uint) id);

        if (classJob == null) {
            throw new ArgumentOutOfRangeException(nameof(id), string.Format(UIStrings.GameClassFactory_NotFoundError, id));
        }

        return new SerializableGameClass {
            Id = id,
            Name = classJob.Value.Name.ToString(),
            Abbreviation = classJob.Value.Abbreviation.ToString(),
            CategoryName = this.GetCategoryName(classJob.Value),
            SortOrder = classJob.Value.UIPriority,
            IconId = 062100 + id,
            ParentClass = (int) classJob.Value.ClassJobParent.RowId,
        };
    }

    private string GetCategoryName(ClassJob classJob) {
        return classJob.Role switch {
            1 => addonTextLoc.JobCategory_Tank,
            2 => addonTextLoc.JobCategory_MeleeDPS,

            // All ranged is role 3, but physranged uses Stat 2
            3 when classJob.PrimaryStat == 2 => addonTextLoc.JobCategory_RangedDPS,
            3 => addonTextLoc.JobCategory_CasterDPS,

            4 => addonTextLoc.JobCategory_Healer,

            // Crafters/gatherers use a different CJC.
            0 when classJob.ClassJobCategory.RowId == 32 => addonTextLoc.JobCategory_DoL,
            0 when classJob.ClassJobCategory.RowId == 33 => addonTextLoc.JobCategory_DoH,

            // fallback?
            _ => string.Empty
        };
    }
}
