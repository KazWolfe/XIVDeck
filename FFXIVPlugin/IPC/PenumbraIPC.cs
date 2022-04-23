using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.IPC;

public class PenumbraIPC {
    public static bool PenumbraEnabled { get; private set; } = false;
    private static ICallGateSubscriber<int>? _penumbraApiVersionSubscriber;
    private static ICallGateSubscriber<string, string>? _penumbraResolveDefaultSubscriber;

    public static int PenumbraApiVersion => _penumbraApiVersionSubscriber?.InvokeFunc() ?? 0;

    public static void Initialize() {
        _penumbraApiVersionSubscriber = Injections.PluginInterface.GetIpcSubscriber<int>("Penumbra.ApiVersion");

        if (PenumbraApiVersion == 3) {
            _penumbraResolveDefaultSubscriber = Injections.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveDefaultPath");
            PenumbraEnabled = true;
            PluginLog.Information("Enabled Penumbra IPC connection!");
        }
    }
    
    public static string ResolvePenumbraPath(string path) {
        return _penumbraResolveDefaultSubscriber?.InvokeFunc(path) ?? path;
    }
}