using System;
using Dalamud.Game.Config;

namespace XIVDeck.FFXIVPlugin.Utils.Game; 

public static class GameConfigExtensions {
    public static IDisposable TemporarySet(this GameConfigSection section, string option, bool value) {
        var oldValue = section.GetBool(option);

        section.Set(option, value);

        return new DisposableWrapper(() => { section.Set(option, oldValue); });
    }
}