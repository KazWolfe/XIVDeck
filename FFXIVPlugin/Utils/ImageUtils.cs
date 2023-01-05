using System.IO;
using Lumina.Data.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class ImageUtils {
    public static Image GetImage(this TexFile tex) {
        // NOTE: Lumina auto-converts this to BGRA32, so we can be hands-off on processing.
        return Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);
    }

    public static byte[] ConvertToPng(this Image image) {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}