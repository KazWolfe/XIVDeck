using FFXIVClientStructs.FFXIV.Client.Game;
using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

public enum CooldownRenderMode {
    None,
    RecastSweep,
    ChargeRing,
    ChargeSweep,
}

public record CooldownGroupDetail {
    public int GroupId;
    public CooldownRenderMode RenderMode;

    public long StartTime;
    public long EndTime;

    public int CurrentCharges;
    public int MaxCharges = 1;

    /// Suppress the cooldown timer label from display. Generally only set on GCD
    public bool HideTimerLabel;
}

[GenerateShape]
public partial record CooldownNotification(int GroupId);

public record ActionCooldownDetail {
    public ActionType ActionType;
    public uint ActionId;

    public CooldownGroupDetail? RecastGroup;
    public CooldownGroupDetail? RechargeGroup;
}
