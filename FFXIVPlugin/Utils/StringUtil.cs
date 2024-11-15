using System.Globalization;
using Lumina.Text.ReadOnly;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class StringUtil {
    public static string ToTitleCase(this string str) {
        return ToTitleCase(str, UIStrings.Culture);
    }

    public static string ToTitleCase(this ReadOnlySeString seString) {
        return ToTitleCase(seString.ToString());
    }

    public static string ToTitleCase(this string str, CultureInfo culture) {
        var textInfo = culture.TextInfo;
        return textInfo.ToTitleCase(str);
    }
}
