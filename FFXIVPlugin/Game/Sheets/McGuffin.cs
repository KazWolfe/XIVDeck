using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

namespace XIVDeck.FFXIVPlugin.Game.Sheets;

[Sheet("McGuffin")]
public class McGuffin : ExcelRow {
    public LazyRow<McGuffinUIData> UIData = default!;
    
    public override void PopulateData(RowParser parser, GameData data, Language language) {
        this.RowId = parser.RowId;
        this.SubRowId = parser.SubRowId;

        this.UIData = new LazyRow<McGuffinUIData>(data, parser.ReadColumn<byte>(0), language);
    }
}

[Sheet("McGuffinUIData")]
public class McGuffinUIData : ExcelRow {
    
    public sbyte SortOrder { get; set; }
    public uint Icon { get; set; }
    public SeString Name { get; private set; } = default!;
    

    public override void PopulateData(RowParser parser, GameData data, Language language) {
        this.RowId = parser.RowId;
        this.SubRowId = parser.SubRowId;

        this.SortOrder = parser.ReadColumn<sbyte>(0);
        this.Icon = parser.ReadColumn<uint>(1);
        this.Name = parser.ReadColumn<SeString>(2) ?? new SeString("");
    }
}
