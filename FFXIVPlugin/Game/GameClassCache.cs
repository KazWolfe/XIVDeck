using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.Game;

[Service(ServiceFlags.Singleton | ServiceFlags.AutoLoad)]
public class GameClassCache {
    private readonly IDataManager _dataManager;
    private readonly GameClassFactory _factory;
    private readonly ILogger _log;

    private List<SerializableGameClass> _cache;

    public GameClassCache(IDataManager dataManager, GameClassFactory factory, ILogger log) {
        this._dataManager = dataManager;
        this._factory = factory;
        this._log = log;

        this._cache = this.Load();
    }

    public List<SerializableGameClass> GetAll() {
        return this._cache;
    }

    public void Reload() {
        this._cache = this.Load();
    }

    private List<SerializableGameClass> Load() {
        var classes = new List<SerializableGameClass>();

        foreach (var gameClass in this._dataManager.GetExcelSheet<ClassJob>()!) {
            classes.Add(this._factory.Create((int) gameClass.RowId));
        }

        this._log.Debug("Populated GameClassCache with {Count} entries.", classes.Count);

        return classes;
    }
}
