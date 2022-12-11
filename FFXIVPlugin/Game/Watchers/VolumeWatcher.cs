using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Logging;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Game.Watchers; 

public class VolumeWatcher : IDisposable {
    private readonly Dictionary<SoundChannel, (int Level, bool Muted)> _volumeCache = new();
    
    public VolumeWatcher() {
        Injections.Framework.Update += this.OnGameUpdate;
    }

    public void Dispose() {
        Injections.Framework.Update -= this.OnGameUpdate;

        GC.SuppressFinalize(this);
    }

    private void OnGameUpdate(Framework framework) {
        foreach (var (channel, (levelOption, muteOption)) in VolumeManager.Channels) {
            var volumeLevel = GameConfig.System.GetUInt(levelOption);
            var muted = GameConfig.System.GetBool(muteOption);

            if (!this._volumeCache.TryGetValue(channel, out var result)) {
                result = (-1, false);
            }

            if (result.Level == volumeLevel && result.Muted == muted) continue;

            result = ((int) volumeLevel, muted);
            PluginLog.Verbose($"Volume update for channel {channel.ToString()}. " +
                              $"Vol = {volumeLevel} Muted = {muted}");
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSVolumeUpdateMessage(channel, volumeLevel, muted));

            this._volumeCache[channel] = result;
        }
    }
}