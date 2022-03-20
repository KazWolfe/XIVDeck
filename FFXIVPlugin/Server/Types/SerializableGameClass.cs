﻿using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Server.Types {
    public class SerializableGameClass {
        private static List<SerializableGameClass> _cache = new();

        public static List<SerializableGameClass> GetCache() {
            if (_cache.Count != 0) return _cache;
            
            foreach (var gameClass in Injections.DataManager.GetExcelSheet<ClassJob>()!) {
                _cache.Add(new SerializableGameClass((int) gameClass.RowId));
            }
                
            PluginLog.Debug($"Populated GameClassCache with {_cache.Count} entries.");

            return _cache;
        }
        
        private static ExcelSheet<ClassJob> _classSheet = Injections.DataManager.GetExcelSheet<ClassJob>()!;
        
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        
        [JsonProperty("categoryName")] public string CategoryName { get; set; }
        [JsonProperty("sortOrder")] public int SortOrder { get; }

        [JsonProperty("iconId")] public int IconId { get; }
        [JsonProperty("iconData")] public string IconData { get; }
        
        [JsonProperty("parentClass")] public int ParentClass { get; set; }
        
        [JsonProperty("hasGearset")] public bool HasGearset { get; set; }
        
        public SerializableGameClass(int id) {
            this.Id = id;
            var classJob = _classSheet.GetRow((uint) id);

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
            this.IconData = XIVDeckPlugin.Instance.IconManager.GetIconAsPngString(this.IconId);

            this.ParentClass = (int) classJob.ClassJobParent.Row;
        }
    }
}