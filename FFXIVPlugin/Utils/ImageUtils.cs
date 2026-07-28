using System.IO;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using Lumina.Data.Files;
using StbImageWriteSharp;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class ImageUtils {
    public static RawImage GetImage(this TexFile tex) {
        var width = tex.Header.Width;
        var height = tex.Header.Height;

        if (tex.Header.Format is TexFile.TextureFormat.BC7) {
            var decoder = new BcDecoder();
            var pixels = decoder.DecodeRaw(tex.TextureBuffer.RawData, width, height, CompressionFormat.Bc7);

            var rgba = new byte[pixels.Length * 4];
            for (var i = 0; i < pixels.Length; i++) {
                rgba[i * 4] = pixels[i].r;
                rgba[i * 4 + 1] = pixels[i].g;
                rgba[i * 4 + 2] = pixels[i].b;
                rgba[i * 4 + 3] = pixels[i].a;
            }

            return new RawImage(rgba, width, height);
        }

        // images are BGRA from Lumina, so convert them over to RGBA.
        var bgra = tex.ImageData;
        var rgbaFromBgra = new byte[bgra.Length];
        for (var i = 0; i < bgra.Length; i += 4) {
            rgbaFromBgra[i] = bgra[i + 2];
            rgbaFromBgra[i + 1] = bgra[i + 1];
            rgbaFromBgra[i + 2] = bgra[i];
            rgbaFromBgra[i + 3] = bgra[i + 3];
        }

        return new RawImage(rgbaFromBgra, width, height);
    }

    public static byte[] ConvertToPng(this RawImage image) {
        using var stream = new MemoryStream();
        var writer = new ImageWriter();
        writer.WritePng(image.Pixels, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, stream);
        return stream.ToArray();
    }
}

public readonly record struct RawImage(byte[] Pixels, int Width, int Height);
