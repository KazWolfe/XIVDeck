using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace XIVDeck.FFXIVPlugin.Game; 

public unsafe class UnlockHelper {
    private static UnlockHelper? _instance;

    public static UnlockHelper GetInstance() {
        return _instance ??= new UnlockHelper();
    }

    private UnlockHelper() {
        SignatureHelper.Initialise(this);
    }

    [Signature("40 57 48 83 EC 20 8D 42 FF 48 8B F9", Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<UIModule*, uint, char> _isMainCommandUnlocked = null;

    public bool IsMainCommandUnlocked(uint commandId) {
        if (this._isMainCommandUnlocked == null || (nint) this._isMainCommandUnlocked == 0) {
            PluginLog.Warning("Signature for IsMainCommandUnlocked was not found!");
            return false;
        }

        return this._isMainCommandUnlocked(Framework.Instance()->UIModule, commandId) > 0;
    }
}