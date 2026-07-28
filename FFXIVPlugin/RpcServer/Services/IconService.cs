using System;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.Utils;
using XIVDeck.FFXIVPlugin.Exceptions;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Icon")]
public class IconService : IDisposable {
    private readonly IconManager _iconManager;

    public event EventHandler<ClearIconCacheMessage>? ClearIconCache;

    public IconService(IconManager iconManager) {
        this._iconManager = iconManager;

        this._iconManager.IconsInvalidated += this.OnIconsInvalidated;
    }

    public void Dispose() {
        this._iconManager.IconsInvalidated -= this.OnIconsInvalidated;

        GC.SuppressFinalize(this);
    }

    public GetIconResponse GetIcon(int iconId) {
        var icon = this._iconManager.GetIcon("", iconId, true);

        if (icon == null) {
            throw new IconNotFoundException(iconId);
        }

        return new GetIconResponse(icon.GetImage().ConvertToPng());
    }

    private void OnIconsInvalidated(object? sender, EventArgs e) {
        this.ClearIconCache?.Invoke(this, new ClearIconCacheMessage());
    }
}
