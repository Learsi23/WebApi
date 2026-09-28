using DashboardEmployee.Exceptions;
using DashboardEmployee.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace DashboardEmployee.Services;
    public class ImageService(IWebHostEnvironment env, ILogger<ImageService> logger) : IImageService
    {

    public const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    // Public URL prefix. Only files under this prefix were created by us and may be deleted.
    private const string UploadsUrlPrefix = "/uploads/employees/";

    // "Magic bytes": the first bytes of a file identify its real format, whatever its name or Content-Type says.
    private static ReadOnlySpan<byte> JpegSignature => [0xFF, 0xD8, 0xFF];
    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private string UploadsFolder => Path.Combine(env.WebRootPath, "uploads", "employees");

    public async Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default)
    {

        if (file.Length == 0)
            throw new BadRequestException("The image file is empty.");
        if (file.Length > MaxFileSizeBytes)
            throw new BadRequestException("The image must be 2 MB or smaller.");
        var header = new byte[12];
        int bytesRead;
        await using (var stream = file.OpenReadStream())
        {
            bytesRead = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        }

        // The extension comes from the content, never from the client's file name.
        var extension = DetectExtension(header.AsSpan(0, bytesRead))
        ?? throw new BadRequestException("Only JPEG, PNG and WebP images are allowed.");

        Directory.CreateDirectory(UploadsFolder);
        // A new GUID name: no collisions and no way for the client to choose the path.
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(UploadsFolder, fileName);
        await using (var output = File.Create(fullPath))
        {
            await file.CopyToAsync(output, ct);
        }
        logger.LogInformation("Saved image {FileName} ({Length} bytes)", fileName, file.Length);

        return UploadsUrlPrefix + fileName;
    }

    public void DeleteImageFile(string? imageUrl)
    {
        // Seed images (/images/...) and anything we did not upload are never touched.
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith(UploadsUrlPrefix, StringComparison.OrdinalIgnoreCase))
            return;

        // GetFileName drops every directory part, so "../../appsettings.json" cannot escape the folder.
        var fileName = Path.GetFileName(imageUrl);
        var fullPath = Path.Combine(UploadsFolder, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            logger.LogInformation("Deleted image {FileName}", fileName);
        }
    }
    private static string? DetectExtension(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith(JpegSignature))
            return ".jpg";
        if (header.StartsWith(PngSignature))
            return ".png";
        // WebP = "RIFF" + 4 bytes of size + "WEBP"
        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) &&
        header[8..12].SequenceEqual("WEBP"u8))
            return ".webp";
        return null;
    }
}