using System;
using Dalamud.Plugin;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class ServiceUtil {
    internal static T GetService<T>(this IDalamudPluginInterface pluginInterface) where T : class {
        return pluginInterface.GetService(typeof(T)) as T ??
               throw new InvalidOperationException($"Service {typeof(T).Name} not found.");
    }
}
