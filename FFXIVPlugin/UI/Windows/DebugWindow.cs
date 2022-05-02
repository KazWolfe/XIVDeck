using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.UI.Windows; 

public class DebugWindow : Window  {
    private const string WindowKey = "XIVDeck Debug Tools";
    
    internal static void InitializeWindow() {
        var windowSystem = XIVDeckPlugin.Instance.WindowSystem;
        var instance = windowSystem.GetWindow(WindowKey);
        
        if (instance == null) {
            windowSystem.AddWindow(new DebugWindow());
        } else {
            instance.IsOpen = true;
        }
    }

    private readonly XIVDeckPlugin _plugin = XIVDeckPlugin.Instance;

    private DebugWindow() : base(WindowKey, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.Size = new Vector2(300, 250);
        this.SizeCondition = ImGuiCond.FirstUseEver;
        
        this.IsOpen = true;
    }
    
    public override void OnClose() {
        this._plugin.WindowSystem.RemoveWindow(this);
    }
    
    public override void Draw() {
        ImGui.Text("Hello, world!");

        if (ImGui.Button("Clear Icons")) {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("DEBUG_ClearIcons"));
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Redraw All Icons")) {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("IconCache"));
        }
    }
}