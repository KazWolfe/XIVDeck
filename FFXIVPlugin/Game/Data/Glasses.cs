using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

namespace XIVDeck.FFXIVPlugin.Game.Data;

[Sheet("Glasses")]
public partial class Glasses : ExcelRow {
    public uint Unknown0 { get; set; }
    public LazyRow<GlassesStyle>? Style { get; set; }
    public int Icon { get; set; }
    public ushort Unknown3 { get; set; }
    public SeString Singular { get; set; } = new("Unknown");
    public sbyte Unknown5 { get; set; }
    public SeString Plural { get; set; } = new("Unknown");
    public sbyte Unknown7 { get; set; }
    public sbyte Unknown8 { get; set; }
    public sbyte Unknown9 { get; set; }
    public sbyte Unknown10 { get; set; }
    public sbyte Unknown11 { get; set; }
    public SeString Description { get; set; } = new("Unknown");
    public SeString Name { get; set; } = new("Unknown");

    public override void PopulateData(RowParser parser, GameData gameData, Language language) {
        base.PopulateData(parser, gameData, language);

        this.Unknown0 = parser.ReadColumn<uint>(0);
        this.Style = new LazyRow<GlassesStyle>(gameData, parser.ReadColumn<short>(1), language);
        this.Icon = parser.ReadColumn<int>(2);
        this.Unknown3 = parser.ReadColumn<ushort>(3);
        this.Singular = parser.ReadColumn<SeString>(4) ?? this.Singular;
        this.Unknown5 = parser.ReadColumn<sbyte>(5);
        this.Plural = parser.ReadColumn<SeString>(6) ?? this.Plural;
        this.Unknown7 = parser.ReadColumn<sbyte>(7);
        this.Unknown8 = parser.ReadColumn<sbyte>(8);
        this.Unknown9 = parser.ReadColumn<sbyte>(9);
        this.Unknown10 = parser.ReadColumn<sbyte>(10);
        this.Unknown11 = parser.ReadColumn<sbyte>(11);
        this.Description = parser.ReadColumn<SeString>(12) ?? this.Description;
        this.Name = parser.ReadColumn<SeString>(13) ?? this.Name;
    }
}

[Sheet("GlassesStyle")]
public partial class GlassesStyle : ExcelRow {
    public short Unknown0 { get; set; }
    public int Icon { get; set; }
    public ushort Order { get; set; }
    public LazyRow<Glasses>[] Glasses { get; set; } = System.Array.Empty<LazyRow<Glasses>>();
    public SeString Singular { get; set; } = new("Unknown");
    public sbyte Unknown16 { get; set; }
    public SeString Plural { get; set; } = new("Unknown");
    public sbyte Unknown18 { get; set; }
    public sbyte Unknown19 { get; set; }
    public sbyte Unknown20 { get; set; }
    public sbyte Unknown21 { get; set; }
    public sbyte Unknown22 { get; set; }
    public SeString Name { get; set; } = new("Unknown");

    public override void PopulateData(RowParser parser, GameData gameData, Language language) {
        base.PopulateData(parser, gameData, language);

        this.Unknown0 = parser.ReadColumn<short>(0);
        this.Icon = parser.ReadColumn<int>(1);
        this.Order = parser.ReadColumn<ushort>(2);
        this.Glasses = new LazyRow<Glasses>[12];
        for (var i = 0; i < 12; i++) this.Glasses[i] = new LazyRow<Glasses>(gameData, parser.ReadColumn<ushort>(3 + i), language);
        this.Singular = parser.ReadColumn<SeString>(15) ?? this.Singular;
        this.Unknown16 = parser.ReadColumn<sbyte>(16);
        this.Plural = parser.ReadColumn<SeString>(17) ?? this.Plural;
        this.Unknown18 = parser.ReadColumn<sbyte>(18);
        this.Unknown19 = parser.ReadColumn<sbyte>(19);
        this.Unknown20 = parser.ReadColumn<sbyte>(20);
        this.Unknown21 = parser.ReadColumn<sbyte>(21);
        this.Unknown22 = parser.ReadColumn<sbyte>(22);
        this.Name = parser.ReadColumn<SeString>(23) ?? this.Name;
    }
}
