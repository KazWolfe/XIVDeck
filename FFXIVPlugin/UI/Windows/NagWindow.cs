using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace XIVDeck.FFXIVPlugin.UI.Windows;

public abstract class NagWindow : XIVDeckWindow {
    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking |
                                                 ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                                                 ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav |
                                                 ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;

    protected abstract void _internalDraw();

    protected NagWindow(UIManager uiManager, string name, int sizeX = 300) : base(uiManager, name, WindowFlags, true) {
        this.Size = new Vector2(sizeX, 100);
        this.SizeCondition = ImGuiCond.Appearing;

        this.RespectCloseHotkey = false;

        this.BgAlpha = 0.95f;

        var viewport = ImGuiHelpers.MainViewport;
        this.Position = new Vector2((viewport.WorkSize.X - sizeX) / 2, (viewport.WorkSize.Y - 100) / 3);
    }

    public override void Draw() {
        ImGui.PushTextWrapPos();
        this._internalDraw();
        ImGui.PopTextWrapPos();
    }
}
