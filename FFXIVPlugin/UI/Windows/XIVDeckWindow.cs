using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace XIVDeck.FFXIVPlugin.UI.Windows;

public abstract class XIVDeckWindow : Window {
    protected readonly UIManager UiManager;

    protected XIVDeckWindow(UIManager uiManager, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None,
        bool forceMainWindow = false) : base(name, flags, forceMainWindow) {
        this.UiManager = uiManager;
    }

    public override void OnClose() {
        base.OnClose();
        this.UiManager.RequestTeardown(this);
    }
}
