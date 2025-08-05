using System.IO;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using Lumina.Data.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class ImageUtils {
    public static Image GetImage(this TexFile tex) {
        if (tex.Header.Format is TexFile.TextureFormat.BC7) {
            var decoder = new BcDecoder();

            return decoder.DecodeRawToImageRgba32(tex.TextureBuffer.RawData, tex.Header.Width, tex.Header.Height, CompressionFormat.Bc7);
        }

        // NOTE: Lumina auto-converts this to BGRA32, so we can be hands-off on processing.
        return Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);
    }

    public static byte[] ConvertToPng(this Image image) {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    public static async Task<byte[]> ConvertToPngAsync(this Image image) {
        await using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);
        return stream.ToArray();
    }
}
