using System;
using System.Collections.Generic;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Server.Types;

public class SerializableGameClass {
    private static readonly List<SerializableGameClass> Cache = new();

    internal static void LoadCache() {
        Cache.Clear();

        foreach (var gameClass in Injections.DataManager.GetExcelSheet<ClassJob>()!) {
            Cache.Add(new SerializableGameClass((int) gameClass.RowId));
        }

        Injections.PluginLog.Debug($"Populated GameClassCache with {Cache.Count} entries.");
    }

    public static List<SerializableGameClass> GetCache() {
        if (Cache.Count == 0)
            LoadCache();

        return Cache;
    }

    private static readonly ExcelSheet<ClassJob> ClassSheet = Injections.DataManager.GetExcelSheet<ClassJob>()!;

    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("abbreviation")] public string Abbreviation { get; set; }

    [JsonProperty("categoryName")] public string CategoryName { get; set; }
    [JsonProperty("sortOrder")] public int SortOrder { get; }

    [JsonProperty("iconId")] public int IconId { get; }

    [JsonProperty("parentClass")] public int ParentClass { get; set; }

    public SerializableGameClass(int id) {
        this.Id = id;
        var classJob = ClassSheet.GetRowOrDefault((uint) id);

        if (classJob == null) {
            throw new ArgumentOutOfRangeException(nameof(id), string.Format(UIStrings.GameClass_NotFoundError, id));
        }

        this.Name = classJob.Value.Name.ToString();
        this.Abbreviation = classJob.Value.Abbreviation.ToString();
        this.CategoryName = (classJob.Value.UIPriority / 10) switch {
            // This is a bit hacky, but eh. This should work until SE breaks their own UI.
            0 => AddonTextLoc.JobCategory_Tank,
            1 => AddonTextLoc.JobCategory_Healer,
            2 => AddonTextLoc.JobCategory_MeleeDPS,
            3 => AddonTextLoc.JobCategory_RangedDPS,
            4 => AddonTextLoc.JobCategory_CasterDPS,
            10 => AddonTextLoc.JobCategory_DoH,
            20 => AddonTextLoc.JobCategory_DoL,

            _ => throw new IndexOutOfRangeException(string.Format(UIStrings.GameClass_UncategorizedError, this.Id))
        };

        this.SortOrder = classJob.Value.UIPriority;
        this.IconId = 062100 + this.Id;

        this.ParentClass = (int)classJob.Value.ClassJobParent.RowId;
    }
}
