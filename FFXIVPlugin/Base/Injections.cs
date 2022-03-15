﻿using System.Diagnostics.CodeAnalysis;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.IoC;
using Dalamud.Plugin;

// disable nullable warnings as all of these are injected. if they're missing, we have serious issues.
#pragma warning disable CS8618

namespace XIVDeck.FFXIVPlugin.Base {
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class Injections {
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static ChatGui Chat { get; private set; }
        [PluginService] public static ClientState ClientState { get; private set; }
        [PluginService] public static CommandManager CommandManager { get; private set; }
        [PluginService] public static Condition Condition { get; private set; }
        [PluginService] public static DataManager DataManager { get; private set; }
        [PluginService] public static Framework Framework { get; private set; }
        [PluginService] public static GameGui GameGui { get; private set; }
        [PluginService] public static KeyState KeyState { get; private set; }
        [PluginService] public static LibcFunction LibcFunction { get; private set; }
        [PluginService] public static ObjectTable Objects { get; private set; }
        [PluginService] public static SigScanner SigScanner { get; private set; }
        [PluginService] public static TargetManager TargetManager { get; private set; }
        [PluginService] public static ToastGui Toasts { get; private set; }    
    }
}