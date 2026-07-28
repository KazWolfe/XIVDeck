using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.IoC;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Contract;


namespace XIVDeck.FFXIVPlugin.Game;

/// <summary>
/// Our own entrypoint into the game's hotbar rendering system.
/// </summary>
[Service(ServiceFlags.Singleton)]
public class ActionAppearanceResolver(CooldownManager cooldownManager, IFramework framework) {
    public Task<ActionAppearance> GetActionAppearance(HotbarSlotType slotType, uint commandId) {
        return framework.RunOnFrameworkThread(() => this.GetVirtualSlotAppearance(slotType, commandId));
    }

    public unsafe ActionAppearance GetAppearance(HotbarSlot* slot) {
        HotbarManager.ResolveApparentAction(slot, out var apparentType, out var apparentId);

        HotbarUIIntermediate intermediate;
        RaptureHotbarModule.Instance()->PopulateIntermediateFromSlot(slot, &intermediate);

        var result = new ActionAppearance {
            IconId = slot->CommandType == HotbarSlotType.Empty ? 0 : slot->GetIconIdForSlot(apparentType, apparentId),
            CooldownDetails = cooldownManager.GetActionCooldownSnapshotForSlot(slot, apparentType, apparentId),
        };

        if (intermediate.CostDisplayMode is >= 1 and <= 4) {
            result.CostText = intermediate.CostDisplayMode is 1 or 3
                ? intermediate.CostValue == 0 ? "" : intermediate.CostValue.ToString()
                : intermediate.CostText.ToString();
            result.CostType = (ActionCostType)intermediate.CostType;
            result.CostRightJustified = intermediate.CostDisplayMode >= 3;
        } else {
            result.CostText = "";
        }

        return result;
    }

    private unsafe ActionAppearance GetVirtualSlotAppearance(HotbarSlotType slotType, uint commandId) {
        var virtualSlot = new HotbarSlot();
        virtualSlot.Initialize();
        virtualSlot.Set(slotType, commandId);

        RaptureHotbarModule.GetSlotAppearance(&virtualSlot.ApparentSlotType, &virtualSlot.ApparentActionId,
            &virtualSlot.ApparentActionModeParam, RaptureHotbarModule.Instance(), &virtualSlot);

        return this.GetAppearance(&virtualSlot);
    }
}
