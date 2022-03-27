using Dalamud.Interface.Colors;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags; 

public class SetupNag : NagWindow {
    private static SetupNag? _instance;

    public static void Show() {
        _instance ??= new SetupNag();
        _instance.IsOpen = true;
    }

    public static void Hide() {
        if (_instance == null) return;
        
        _instance.Dispose();
        _instance = null;
    }

    public SetupNag() : base("sdPluginNotInstalled", 400, 325) { }

    protected override void _internalDraw() {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text("A Stream Deck has never connected to the game.");
        ImGui.PopStyleColor();
        
        ImGui.Separator();
        
        ImGui.Text("If you haven't done so already, please make sure you've downloaded and installed the " +
                   "companion XIVDeck Stream Deck Plugin from GitHub.");
        
        if (ImGui.Button("Open XIVDeck Download Page")) {
            PluginUI.OpenXIVDeckGitHub($"/releases/tag/v{StringUtils.GetMajMinRev()}");
        }
            
        ImGui.Spacing();
        
        ImGui.Text("If the XIVDeck Stream Deck Plugin is already installed, please make sure the port is set " +
                   "correctly in the configuration and that at least one button exists.");
        
        ImGui.Text($"Current port: {XIVDeckPlugin.Instance.Configuration.WebSocketPort}");
        
        ImGui.Text("If you need to change the port the server is hosted on, you may do so from XIVDeck's settings.");
        if (ImGui.Button("Open XIVDeck Settings")) {
            XIVDeckPlugin.Instance.DrawConfigUI();
        }
            
        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "To dismiss this message, resolve the above problem or uninstall the XIVDeck plugin.");
    }
}