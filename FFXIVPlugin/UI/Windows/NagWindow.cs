using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;

namespace XIVDeck.FFXIVPlugin.UI.Windows;

public abstract class NagWindow : Window {
    internal static void CloseAllNags() {
        ForcedUpdateNag.Hide();
        SetupNag.Hide();
        PortInUseNag.Hide();
        TestingUpdateNag.Hide();
        MultiboxNag.Hide();
    }

    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking |
                                                 ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                                                 ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav |
                                                 ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;

    protected abstract void _internalDraw();

    protected NagWindow(string name, int sizeX = 300) : base(name, WindowFlags, true) {
        this.Size = new Vector2(sizeX, 100);
        this.SizeCondition = ImGuiCond.Appearing;

        this.RespectCloseHotkey = false;

        this.BgAlpha = 0.95f;

        var viewport = ImGuiHelpers.MainViewport;
        this.Position = new Vector2((viewport.WorkSize.X - sizeX) / 2, (viewport.WorkSize.Y - 100) / 3);

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
