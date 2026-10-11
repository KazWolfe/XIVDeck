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

        switch (intermediate.CostDisplayMode)
        {
            // TODO: Eventually .CostValue will be an int. Maybe.
            case 1 or 3 when intermediate.CostValue is not (0 or uint.MaxValue):
                result.CostText = intermediate.CostValue.ToString();
                result.CostType = (ActionCostType)intermediate.CostType;
                result.CostRightJustified = intermediate.CostDisplayMode >= 3;
                break;
            case 2 or 4:
                result.CostText = intermediate.CostText.ToString();
                result.CostType = (ActionCostType)intermediate.CostType;
                result.CostRightJustified = intermediate.CostDisplayMode >= 3;
                break;
            default:
                result.CostText = "";
                break;
        }

        return result;
    }

    private unsafe ActionAppearance GetVirtualSlotAppearance(HotbarSlotType slotType, uint commandId) {
        var virtualSlot = new HotbarSlot();
        virtualSlot.Initialize();

        try {
            virtualSlot.Set(slotType, commandId);

            RaptureHotbarModule.GetSlotAppearance(&virtualSlot.ApparentSlotType, &virtualSlot.ApparentActionId,
                &virtualSlot.ApparentActionModeParam, RaptureHotbarModule.Instance(), &virtualSlot);

            var appearance = this.GetAppearance(&virtualSlot);

            return appearance;
        } finally {
            // note: rare memleak when a hotbar slot has a really long name.
            virtualSlot.Clear();
            virtualSlot.PopUpHelp.Dtor();
        }
    }
}
