using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("ClassJob")]
public class ClassJobService {
    private readonly ILogger _log;
    private readonly IClientState _clientState;
    private readonly IFramework _framework;
    private readonly IDataManager _dataManager;
    private readonly ErrorNotifier _errorNotifier;
    private readonly GameClassCache _gameClassCache;

    public ClassJobService(ILogger log, IClientState clientState, IFramework framework, IDataManager dataManager,
        ErrorNotifier errorNotifier, GameClassCache gameClassCache) {
        this._log = log;
        this._clientState = clientState;
        this._framework = framework;
        this._dataManager = dataManager;
        this._errorNotifier = errorNotifier;
        this._gameClassCache = gameClassCache;
    }

    public GetClassesResponse GetClasses() {
        return new GetClassesResponse(this._gameClassCache.GetAll());
    }

    public GetClassesResponse GetAvailableClasses() {
        var availableClasses = GearsetManager.GetGearsets()
            .Select(gearset => (int)gearset.ClassJob)
            .ToList();

        return new GetClassesResponse(
            this._gameClassCache.GetAll().Where(gameClass => availableClasses.Contains(gameClass.Id)).ToList());
    }

    public SerializableGameClass GetClass(int id) {
        return this._gameClassCache.GetAll()[id];
    }

    public async Task SwitchClass(int id) {
        if (id < 1)
            throw new ActionInvalidException(UIStrings.ClassJobService_ClassLessThan1Error);

        if (!this._clientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();

        var sheet = this._dataManager.Excel.GetSheet<ClassJob>();
        var classJob = sheet.GetRowOrDefault((uint)id);

        if (classJob == null)
            throw new ActionNotFoundException(string.Format(UIStrings.ClassJobService_InvalidClassIdError, id));

        GameUtils.SendDummyInput();

        while (true) {
            foreach (var gearset in GearsetManager.GetGearsets()) {
                if (gearset.ClassJob != id) continue;

                await this._framework.RunOnFrameworkThread(delegate {
                    var command = $"/gs change {gearset.Slot}";
                    this._log.Debug("Sending command: {Command}", command);
                    ChatHelper.SendSanitizedChatMessage(command);
                });

                // notify the user on fallback
                if (id != classJob.Value.RowId) {
                    var fallbackClassJob = sheet.GetRow((uint)id);

                    this._log.Information(
                        "Used fallback {FallbackClass} for requested {RequestedClass}",
                        fallbackClassJob.Abbreviation, classJob.Value.Abbreviation);
                    this._errorNotifier.ShowError(string.Format(
                        UIStrings.ClassJobService_FallbackClassUsed,
                        classJob.Value.Name.ToTitleCase(), fallbackClassJob.Name.ToTitleCase()), true);
                }

                return;
            }

            // fallback logic
            var parentId = classJob.Value.ClassJobParent.RowId;
            if (parentId == id || parentId == 0) {
                this._log.Debug("Couldn't find a fallback class for {RequestedClass}", classJob.Value.Abbreviation);
                break;
            }

            id = (int)parentId;
        }

        throw new IllegalGameStateException(
            string.Format(
                UIStrings.ClassJobService_NoGearsetForClassError,
                UIStrings.Culture.TextInfo.ToTitleCase(classJob.Value.Name.ExtractText())));
    }
}
