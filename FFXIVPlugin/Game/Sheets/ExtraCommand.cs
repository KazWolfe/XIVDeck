using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

namespace XIVDeck.FFXIVPlugin.Game.Sheets; 

[Sheet("ExtraCommand")]
public class ExtraCommand : ExcelRow {
    public SeString Name { get; private set; } = default!;
    public SeString Description { get; private set; } = default!;
        
    public int Icon { get; set; }
    public sbyte UIPriority { get; set; }
        
    public override void PopulateData(RowParser parser, GameData data, Language language) {
        this.RowId = parser.RowId;
        this.SubRowId = parser.SubRowId;

        this.Name = parser.ReadColumn<SeString>(0) ?? new SeString("");
        this.Description = parser.ReadColumn<SeString>(1) ?? new SeString("");
        this.Icon = parser.ReadColumn<int>(2);
        this.UIPriority = parser.ReadColumn<sbyte>(3);
    }
}