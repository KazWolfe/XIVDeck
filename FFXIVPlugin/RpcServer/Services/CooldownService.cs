using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using CooldownNotification = XIVDeck.FFXIVPlugin.Contract.CooldownNotification;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Cooldown")]
public class CooldownService : IDisposable {
    private readonly IFramework _framework;
    private readonly CooldownManager _cooldownManager;

    public event EventHandler<CooldownNotification>? CooldownStart;

    public CooldownService(IFramework framework, CooldownManager cooldownManager) {
        this._framework = framework;
        this._cooldownManager = cooldownManager;

        this._cooldownManager.CooldownStart += this.OnCooldownStart;
    }

    public void Dispose() {
        this._cooldownManager.CooldownStart -= this.OnCooldownStart;

        GC.SuppressFinalize(this);
    }

    public async Task<ActionCooldownDetail> GetActionCooldown(string actionType, uint actionId) {
        if (!Enum.TryParse<ActionType>(actionType, out var type)) {
            throw new ArgumentException(@$"Unknown action type {actionType}", nameof(actionType));
        }

        return await this._framework.RunOnFrameworkThread(() =>
            CooldownManager.GetActionCooldownSnapshot(type, actionId)
        );
    }

    private void OnCooldownStart(object? sender, CooldownNotification detail) {
        this.CooldownStart?.Invoke(this, detail);
    }
}
