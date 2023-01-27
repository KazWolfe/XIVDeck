using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;

namespace XIVDeck.FFXIVPlugin.UI.Windows; 

public abstract class NagWindow : Window {
    internal static void CloseAllNags() {
        ForcedUpdateNag.Hide();
        SetupNag.Hide();
        PortInUseNag.Hide();
        TestingUpdateNag.Hide();
    }
    
    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking |
                                                 ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | 
                                                 ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav |
                                                 ImGuiWindowFlags.NoTitleBar;

    protected abstract void _internalDraw();

    protected NagWindow(string name, int sizeX = 300, int sizeY = 250) : base(name, WindowFlags, true) {
        this.Size = new Vector2(sizeX, sizeY);
        this.SizeCondition = ImGuiCond.Always;
        
        this.RespectCloseHotkey = false;

        this.BgAlpha = 0.85f;

        var viewport = ImGuiHelpers.MainViewport;
        this.Position = new Vector2((viewport.WorkSize.X - sizeX) / 2, (viewport.WorkSize.Y - sizeY) / 3);

        // automatically open a nag window on creation
        XIVDeckPlugin.Instance.WindowSystem.AddWindow(this);
        this.IsOpen = true;
    }
    
    public override void Draw() {
        ImGui.PushTextWrapPos();
        this._internalDraw();
        ImGui.PopTextWrapPos();
    }
}