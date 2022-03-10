using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class SerializableGameClass {
        private static List<SerializableGameClass> _cache = new();

        public static List<SerializableGameClass> GetCache() {
            if (_cache.Count == 0) {
                foreach (var gameClass in Injections.DataManager.GetExcelSheet<ClassJob>()!) {
                    _cache.Add(new SerializableGameClass((int) gameClass.RowId));
                }
                
                PluginLog.Debug($"Populated GameClassCache with {_cache.Count} entries.");
            }

            return _cache;
        }
        
        private static ExcelSheet<ClassJob> _classSheet = Injections.DataManager.GetExcelSheet<ClassJob>();
        
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        
        [JsonProperty("categoryName")] public string CategoryName { get; set; }
        [JsonProperty("sortOrder")] public int SortOrder { get; }

        [JsonProperty("iconId")] public int IconId { get; }
        [JsonProperty("iconString")] public string IconData { get; }
        
        [JsonProperty("parentClass")] public int ParentClass { get; set; }
        
        [JsonProperty("hasGearset")] public bool HasGearset { get; set; }

        [NonSerialized] private ClassJob _classJob;

        public SerializableGameClass(int id) {
            this.Id = id;
            this._classJob = _classSheet!.GetRow((uint) id);

            if (this._classJob == null) {
                throw new ArgumentOutOfRangeException(nameof(id), $"A class with ID {id} does not exist.");
            }

            this.Name = this._classJob.Name.RawString;
            this.CategoryName = (this._classJob.UIPriority / 10) switch {
                0 => "Tank",
                1 => "Healer",
                2 => "Melee DPS",
                3 => "Ranged DPS",
                4 => "Caster DPS",
                10 => "Disciple of the Hand",
                20 => "Disciple of the Land",
                
                _ => throw new IndexOutOfRangeException($"Unrecognized job category for class ID: {this.Id}")
            };

            this.SortOrder = this._classJob.UIPriority;
            this.IconId = 062100 + this.Id;
            this.IconData = XIVDeckPlugin.Instance.IconManager.GetIconAsPngString(this.IconId);

            this.ParentClass = (int) this._classJob.ClassJobParent.Row;
        }
    }
    
    public class WSGetClassesOpcode : BaseInboundMessage {
        private readonly GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        private readonly List<SerializableGameClass> _gameClassCache = SerializableGameClass.GetCache();
        
        public override void Process(WsSession session) {
            this._gameStateCache.Refresh();

            var reply = new Dictionary<string, dynamic> {
                ["messageType"] = "gameClasses",
                ["classes"] = this._gameClassCache,
                ["available"] = this._gameStateCache.Gearsets!.Select(gearset => (int) gearset.ClassJob).ToList()
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }

        public WSGetClassesOpcode() : base("getClasses") {

        }

    }
}