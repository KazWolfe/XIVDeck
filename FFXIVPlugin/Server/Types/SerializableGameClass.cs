using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Server.Types; 

public class SerializableGameClass {
    private static readonly List<SerializableGameClass> Cache = new();

    public static List<SerializableGameClass> GetCache() {
        if (Cache.Count != 0) return Cache;
            
        foreach (var gameClass in Injections.DataManager.GetExcelSheet<ClassJob>()!) {
            Cache.Add(new SerializableGameClass((int) gameClass.RowId));
        }
                
        PluginLog.Debug($"Populated GameClassCache with {Cache.Count} entries.");

        return Cache;
    }
        
    private static readonly ExcelSheet<ClassJob> ClassSheet = Injections.DataManager.GetExcelSheet<ClassJob>()!;
        
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
        
    [JsonProperty("categoryName")] public string CategoryName { get; set; }
    [JsonProperty("sortOrder")] public int SortOrder { get; }

    [JsonProperty("iconId")] public int IconId { get; }
        
    [JsonProperty("parentClass")] public int ParentClass { get; set; }
        
    [JsonProperty("hasGearset")] public bool HasGearset { get; set; }
        
    public SerializableGameClass(int id) {
        this.Id = id;
        var classJob = ClassSheet.GetRow((uint) id);

        if (classJob == null) {
            throw new ArgumentOutOfRangeException(nameof(id), $"A class with ID {id} does not exist.");
        }

        this.Name = classJob.Name.RawString;
        this.CategoryName = (classJob.UIPriority / 10) switch {
            0 => "Tank",
            1 => "Healer",
            2 => "Melee DPS",
            3 => "Ranged DPS",
            4 => "Caster DPS",
            10 => "Disciple of the Hand",
            20 => "Disciple of the Land",
                
            _ => throw new IndexOutOfRangeException($"Unrecognized job category for class ID {this.Id}. REPORT THIS BUG!")
        };

        this.SortOrder = classJob.UIPriority;
        this.IconId = 062100 + this.Id;
            
        this.ParentClass = (int) classJob.ClassJobParent.Row;
    }
}