using Lumina;
using Lumina.Data;
using Lumina.Excel;

namespace XIVDeck.FFXIVPlugin.Game.Data;

public class Marker : Lumina.Excel.GeneratedSheets.Marker {
    public byte SortOrder { get; private set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language) {
        base.PopulateData(parser, gameData, language);

        this.SortOrder = parser.ReadColumn<byte>(1);
    }
}