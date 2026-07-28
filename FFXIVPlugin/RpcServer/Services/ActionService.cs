using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Nerdbank.MessagePack;
using Serilog;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Types;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Contract;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Action")]
public class ActionService : IDisposable {
    private readonly ILogger _log;
    private readonly IClientState _clientState;
    private readonly ActionDispatcher _dispatcher;
    private readonly ActionUpdateHooks _updateHooks;

    public event EventHandler<ActionTypeUpdateBatch>? ActionTypeUpdate;

    public ActionService(ILogger logger, IClientState clientState, ActionDispatcher dispatcher,
        ActionUpdateHooks updateHooks) {
        this._log = logger;
        this._clientState = clientState;
        this._dispatcher = dispatcher;
        this._updateHooks = updateHooks;

        this._updateHooks.ActionUpdate += this.OnActionUpdate;
    }

    public void Dispose() {
        this._updateHooks.ActionUpdate -= this.OnActionUpdate;
        GC.SuppressFinalize(this);
    }

    private void OnActionUpdate(object? sender, IEnumerable<ActionUpdateEvent> updateEvents) {
        this.ActionTypeUpdate?.Invoke(this, new ActionTypeUpdateBatch([
            .. updateEvents.Select(updateEvent => new ActionTypeUpdateNotification {
                UpdatedType = updateEvent.SlotType,
                ActionId = updateEvent.ActionId
            })
        ]));
    }

    public GetActionsResponse GetActions() {
        Dictionary<string, List<ActionEntry>> actions = new();

        foreach (var (type, strategy) in this._dispatcher.GetStrategies()) {
            var allowedItems = strategy.GetSelectableActions();
            if (allowedItems.Count == 0) continue;

            actions[type.ToString()] = allowedItems;
        }

        return new GetActionsResponse(actions);
    }

    public GetActionsByTypeResponse GetActionsByType(string type) {
        var strategy = this.GetStrategyForTypeName(type, out _);
        return new GetActionsByTypeResponse(strategy.GetSelectableActions());
    }

    public ActionEntry GetActionEntry(string type, int id) {
        var strategy = this.GetStrategyForTypeName(type, out var slotType);
        var entry = strategy.GetActionEntryById((uint)id);

        if (entry == null) {
            throw new ActionNotFoundException(slotType, (uint)id);
        }

        return entry;
    }

    public async Task<ActionAppearance> GetActionAppearance(string type, int id) {
        var strategy = this.GetStrategyForTypeName(type, out var slotType);

        if (strategy.GetActionEntryById((uint)id) == null) {
            throw new ActionNotFoundException(slotType, (uint)id);
        }

        return await strategy.GetAppearance((uint)id);
    }

    public async Task ExecuteAction(string type, int id, RawMessagePack? payload = null) {
        var strategy = this.GetStrategyForTypeName(type, out _);

        if (!this._clientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();

        var payloadType = strategy.GetPayloadType();

        ActionPayload? actionPayload = null;
        if (payloadType != null && payload != null) {
            var shape = Witness.GeneratedTypeShapeProvider.GetTypeShape(payloadType)!;
            actionPayload =
                RpcFormatter.UserDataSerializer.DeserializeObject(payload.Value.MsgPack, shape) as ActionPayload;

            this._log.Debug("Payload: {ActionPayload}", actionPayload);
        }

        GameUtils.SendDummyInput();
        await strategy.Execute((uint)id, actionPayload);
    }

    private IActionStrategy GetStrategyForTypeName(string typeName, out HotbarSlotType slotType) {
        if (!TryGetSlotTypeByName(typeName, out slotType) || !this._dispatcher.TryGetStrategyForType(slotType, out var strategy)) {
            throw new ActionTypeNotFoundException(typeName);
        }

        return strategy;
    }

    private static bool TryGetSlotTypeByName(string typeName, out HotbarSlotType slotType) {
        return Enum.TryParse(typeName, out slotType) && Enum.GetName(slotType) == typeName;
    }
}
