using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

public enum ActionCostType : byte {
    Default = 0,
    Health = 1,
    Magic = 2,
    Tactical = 3,
    Crafting = 4,
    Gathering = 5,
    JobGauge = 6,
    Ceruleum = 7,
}

[GenerateShape]
public partial record ActionAppearance {
    // the actual raw icon ID
    public int IconId;

    public string? CostText;
    public ActionCostType CostType;
    public bool CostRightJustified;

    // used to pass the cooldown group id and initial data
    public ActionCooldownDetail? CooldownDetails;
}
