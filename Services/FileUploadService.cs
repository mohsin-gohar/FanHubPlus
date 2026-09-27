using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FanHubPlus.Services;

/// <summary>
/// Secure image upload:
///   1. size limit (configurable, default 2 MiB)
///   2. extension whitelist
///   3. client MIME whitelist
///   4. REAL file-content check (magic bytes) - a .php/.exe renamed to .jpg is rejected
///   5. image dimensions read from the header (max 6000x6000) - blocks decompression bombs
///   6. random server-generated filename (no user input, no traversal, no overwrite)
///   7. files land ONLY inside wwwroot/uploads/{subFolder}
/// </summary>
public class FileUploadService : IFileUploadService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly string[] AllowedContentTypes =
        { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };

    private readonly IWebHostEnvironment _env;
    private readonly long _maxBytes;
    private readonly int _maxWidth;
    private readonly int _maxHeight;

    public FileUploadService(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        // 2 * 1024 * 1024 = 2 MiB (the old constant 2*1024*1000 silently capped at ~1.95 MiB)
        _maxBytes = config.GetValue("FileUploads:MaxImageBytes", 2 * 1024 * 1024);
        _maxWidth = config.GetValue("FileUploads:MaxWidth", 6000);
        _maxHeight = config.GetValue("FileUploads:MaxHeight", 6000);
    }

    public async Task<string> SaveImageAsync(IFormFile? file, string subFolder)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("Please choose an image file.");

        if (file.Length > _maxBytes)
            throw new InvalidOperationException(
                $"Image is too large (maximum {_maxBytes / (1024.0 * 1024.0):0.#} MB).");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Only jpg, jpeg, png, webp and gif images are allowed.");

        var declaredType = (file.ContentType ?? string.Empty).ToLowerInvariant();
        if (!AllowedContentTypes.Contains(declaredType))
            throw new InvalidOperationException("File content type is not an allowed image type.");

        // ---- read the header once: magic bytes (+ dimensions where available) ----
        var header = new byte[32];
        await using (var probe = file.OpenReadStream())
        {
            var read = await probe.ReadAsync(header.AsMemory(0, header.Length));
            if (read < 12) throw new InvalidOperationException("The selected file is not a valid image.");
        }

        var detected = DetectFormat(header);
        if (detected is null)
            throw new InvalidOperationException("The file content is not a real JPG, PNG, WEBP or GIF image.");

        if (!ExtensionMatches(ext, detected.Value))
            throw new InvalidOperationException("The file extension does not match the image content.");

        var (width, height) = ReadDimensions(header, detected.Value, file);
        if (width > _maxWidth || height > _maxHeight)
            throw new InvalidOperationException(
                $"Image dimensions are too large (maximum {_maxWidth}x{_maxHeight} pixels).");

        // ---- sanitise folder + collision-proof server-side name ----
        subFolder = new string(subFolder.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
        if (string.IsNullOrEmpty(subFolder)) subFolder = "misc";

        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
        var folderAbs = Path.GetFullPath(Path.Combine(uploadsRoot, subFolder));
        Directory.CreateDirectory(folderAbs);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.GetFullPath(Path.Combine(folderAbs, fileName));

        // defense in depth: never write outside wwwroot/uploads
        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid upload location.");

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subFolder}/{fileName}";
    }

    public void DeleteImage(string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath) || !webPath.StartsWith("/uploads/", StringComparison.Ordinal))
            return; // never delete outside the uploads folder

        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
        var target = Path.GetFullPath(Path.Combine(_env.WebRootPath,
            webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        var rootPrefix = uploadsRoot.EndsWith(Path.DirectorySeparatorChar)
            ? uploadsRoot
            : uploadsRoot + Path.DirectorySeparatorChar;
        if (!target.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(target)) File.Delete(target);
    }

    // ---------------------------------------------------------------- helpers

    private enum ImageFormat { Jpeg, Png, Gif, Webp }

    /// <summary>Sniffs the real format from the first bytes of the file.</summary>
    private static ImageFormat? DetectFormat(byte[] h)
    {
        if (h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ImageFormat.Jpeg;
        if (h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return ImageFormat.Png;
        if (h[0] == 0x47 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x38) return ImageFormat.Gif;
        if (h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 &&
            h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) return ImageFormat.Webp;
        return null;
    }

    private static bool ExtensionMatches(string ext, ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => ext is ".jpg" or ".jpeg",
        ImageFormat.Png => ext == ".png",
        ImageFormat.Gif => ext == ".gif",
        ImageFormat.Webp => ext == ".webp",
        _ => false
    };

    /// <summary>
    /// Reads width/height from the header where the format allows it.
    /// (0,0) means "unknown" - the magic-byte and size checks have already passed,
    /// so an unreadable size never rejects an otherwise valid image.
    /// </summary>
    private static (int width, int height) ReadDimensions(byte[] header, ImageFormat format, IFormFile file)
    {
        try
        {
            switch (format)
            {
                case ImageFormat.Png:  // IHDR width/height, big-endian, offset 16..23
                    return (ReadInt32Be(header, 16), ReadInt32Be(header, 20));

                case ImageFormat.Gif:  // little-endian 16-bit, offset 6..9
                    return (header[6] | (header[7] << 8), header[8] | (header[9] << 8));

                case ImageFormat.Webp: // VP8X stores a 24-bit canvas size at offset 24..29
                    return header[12] == 0x56 && header[13] == 0x50 && header[14] == 0x38 && header[15] == 0x58
                        ? (ReadInt24Le(header, 24), ReadInt24Le(header, 27))
                        : (0, 0); // VP8 / VP8L variants: skip the size check

                case ImageFormat.Jpeg:
                    return ReadJpegDimensions(file);

                default:
                    return (0, 0);
            }
        }
        catch
        {
            return (0, 0); // never fail an upload because of a header parsing quirk
        }
    }

    private static int ReadInt32Be(byte[] b, int i) => (b[i] << 24) | (b[i + 1] << 16) | (b[i + 2] << 8) | b[i + 3];
    private static int ReadInt24Le(byte[] b, int i) => b[i] | (b[i + 1] << 8) | (b[i + 2] << 16);

    /// <summary>Walks the JPEG segment chain up to the SOFn marker that carries the size.</summary>
    private static (int width, int height) ReadJpegDimensions(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        stream.Position = 2; // skip the SOI marker (FF D8)

        while (stream.Position < stream.Length - 9)
        {
            if (stream.ReadByte() != 0xFF) continue;

            int type = stream.ReadByte();
            while (type == 0xFF) type = stream.ReadByte(); // padding
            if (type == 0xD8 || type == 0x01 || type is >= 0xD0 and <= 0xD7) continue; // no payload

            int length = (stream.ReadByte() << 8) | stream.ReadByte();

            // SOF0..SOF15 (0xC0-0xCF) except DHT (C4), JPG (C8), DAC (CC) carry the frame size
            if (type is >= 0xC0 and <= 0xCF && type is not 0xC4 and not 0xC8 and not 0xCC)
            {
                stream.ReadByte(); // sample precision
                int height = (stream.ReadByte() << 8) | stream.ReadByte();
                int width = (stream.ReadByte() << 8) | stream.ReadByte();
                return (width, height);
            }

            stream.Position += length - 2;
        }

        return (0, 0);
    }
}
