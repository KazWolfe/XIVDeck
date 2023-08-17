using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace XIVDeck.FFXIVPlugin.Game; 

public unsafe class TempSigs {
    private class Signatures {
        public const string GetActionTypeForSlotType = "FF CA 83 FA 1E";
        public const string GetAdditionalRecastGroup = "E8 ?? ?? ?? ?? 8B 4F 44 33 D2";
    }

    public TempSigs() {
        SignatureHelper.Initialise(this);
    }

    [Signature(Signatures.GetActionTypeForSlotType)]
    private readonly delegate* unmanaged<HotBarSlot*, HotbarSlotType, ActionType> _getActionTypeForSlotType = null;

    [Signature(Signatures.GetAdditionalRecastGroup)]
    private readonly delegate* unmanaged<ActionManager*, ActionType, uint, int> _getAdditionalRecastGroup = null;

    public ActionType GetActionTypeForHotbarSlotType(HotbarSlotType type) {
        if (this._getActionTypeForSlotType == null) return 0;

        return this._getActionTypeForSlotType(null, type);
    }

    public int GetAdditionalRecastGroup(ActionType type, uint actionId) {
        if (this._getAdditionalRecastGroup == null) return -1;
        
        return this._getAdditionalRecastGroup(ActionManager.Instance(), type, actionId);
    }
}