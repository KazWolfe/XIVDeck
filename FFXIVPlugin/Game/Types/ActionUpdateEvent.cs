using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace XIVDeck.FFXIVPlugin.Game.Types;

public record ActionUpdateEvent {
    public required RaptureHotbarModule.HotbarSlotType SlotType;
    public uint? ActionId;
}
