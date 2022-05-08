using System.Globalization;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows; 

// ReSharper disable once UnusedType.Global - Used in #DEBUG builds from SettingsWindow
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
        ImGui.Text("Hello, world! Enjoying your time behind the curtain?");

        if (ImGui.Button("Clear Icons")) {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("DEBUG_ClearIcons"));
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Redraw All Icons")) {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("IconCache"));
        }

        if (ImGui.Button("Resend Init Command")) {
            var xivPluginVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            var packet = new WSInitReplyMessage(xivPluginVersion.GetMajMinBuild(), AuthHelper.Instance.Secret);
            XIVDeckWSServer.Instance?.BroadcastMessage(packet);
        }
        
        ImGui.Spacing();
        
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        if (ImGui.Button("KILL SERVER")) {
            var field = XIVDeckPlugin.Instance.GetType().GetField("_xivDeckWebServer",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var ws = (XIVDeckWebServer) field?.GetValue(XIVDeckPlugin.Instance)!;
            ws.Dispose();
        }
        ImGui.PopStyleColor();
        ImGui.SameLine();
        if (ImGui.Button("(Re)Start Server")) {
            XIVDeckPlugin.Instance.InitializeWebServer();
        }
        
        ImGui.Spacing();
        if (ImGui.Button("Pseudo-localize")) {
            UIStrings.Culture = new CultureInfo("qps-ploc");
        }
    }
}