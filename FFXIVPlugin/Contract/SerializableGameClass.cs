using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

/// <summary>
/// Wire format for a ClassJob. Built by <see cref="GameClassFactory"/>.
/// </summary>
[GenerateShape]
public partial class SerializableGameClass {
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Abbreviation { get; init; } = null!;

    public string CategoryName { get; init; } = null!;
    public int SortOrder { get; init; }

    public int IconId { get; init; }

    public int ParentClass { get; init; }
}
