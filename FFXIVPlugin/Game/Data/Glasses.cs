using System;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

namespace XIVDeck.FFXIVPlugin.Game.Data;

[Sheet("Glasses")]
public class Glasses : ExcelRow {
    public LazyRow<GlassesStyle>? Style { get; set; }
    public int Icon { get; set; }
    public SeString Singular { get; set; } = new("Unknown");
    public SeString Plural { get; set; } = new("Unknown");
    public SeString Description { get; set; } = new("Unknown");
    public SeString Name { get; set; } = new("Unknown");

    public override void PopulateData(RowParser parser, GameData gameData, Language language) {
        base.PopulateData(parser, gameData, language);

        this.Style = new LazyRow<GlassesStyle>(gameData, parser.ReadColumn<short>(1), language);
        this.Icon = parser.ReadColumn<int>(2);
        this.Singular = parser.ReadColumn<SeString>(4) ?? this.Singular;
        this.Plural = parser.ReadColumn<SeString>(6) ?? this.Plural;
        this.Description = parser.ReadColumn<SeString>(12) ?? this.Description;
        this.Name = parser.ReadColumn<SeString>(13) ?? this.Name;
    }
}

[Sheet("GlassesStyle")]
public class GlassesStyle : ExcelRow {
    public ushort Order { get; set; }
    public LazyRow<Glasses>[] Glasses { get; set; } = Array.Empty<LazyRow<Glasses>>();
    public SeString Singular { get; set; } = new("Unknown");
    public SeString Plural { get; set; } = new("Unknown");
    public SeString Name { get; set; } = new("Unknown");

    public override void PopulateData(RowParser parser, GameData gameData, Language language) {
        base.PopulateData(parser, gameData, language);

        this.Order = parser.ReadColumn<ushort>(2);
        this.Glasses = new LazyRow<Glasses>[12];
        for (var i = 0; i < 12; i++) this.Glasses[i] = new LazyRow<Glasses>(gameData, parser.ReadColumn<ushort>(3 + i), language);
        this.Singular = parser.ReadColumn<SeString>(15) ?? this.Singular;
        this.Plural = parser.ReadColumn<SeString>(17) ?? this.Plural;
        this.Name = parser.ReadColumn<SeString>(23) ?? this.Name;
    }
}
