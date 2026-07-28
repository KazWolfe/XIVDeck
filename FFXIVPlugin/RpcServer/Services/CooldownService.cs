using System;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using CooldownNotification = XIVDeck.FFXIVPlugin.Contract.CooldownNotification;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Cooldown")]
public class CooldownService : IDisposable {
    private readonly CooldownManager _cooldownManager;

    public event EventHandler<CooldownNotification>? CooldownStart;

    public CooldownService(CooldownManager cooldownManager) {
        this._cooldownManager = cooldownManager;

        this._cooldownManager.CooldownStart += this.OnCooldownStart;
    }

    public void Dispose() {
        this._cooldownManager.CooldownStart -= this.OnCooldownStart;

        GC.SuppressFinalize(this);
    }

    private void OnCooldownStart(object? sender, CooldownNotification detail) {
        this.CooldownStart?.Invoke(this, detail);
    }
}
