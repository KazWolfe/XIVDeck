using System.Diagnostics.CodeAnalysis;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

// disable nullable warnings as all of these are injected. if they're missing, we have serious issues.
#pragma warning disable CS8618

namespace XIVDeck.FFXIVPlugin.Base; 

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class Injections {
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static ChatGui Chat { get; private set; }
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static ICommandManager CommandManager { get; private set; }
    [PluginService] public static Condition Condition { get; private set; }
    [PluginService] public static IDataManager DataManager { get; private set; }
    [PluginService] public static Framework Framework { get; private set; }
    [PluginService] public static IGameConfig GameConfig { get; private set; }
    [PluginService] public static IGameGui GameGui { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }
    [PluginService] public static ToastGui Toasts { get; private set; }    
}