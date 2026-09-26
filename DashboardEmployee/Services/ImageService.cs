using Microsoft.AspNetCore.Hosting;
using DashboardEmployee.Services.Interfaces;

namespace DashboardEmployee.Services
{
    public class ImageService(IWebHostEnvironment env) : IImageService
    {
        private readonly IWebHostEnvironment _env = env;

        /// <summary>
        /// Validates, decodes, and persists image input (URL, Base64 payload, or null).
        /// </summary>
        public async Task<string> SaveImageAsync(string? inputImage, CancellationToken ct = default)
        {
            // Step 1: Return default avatar if input is null, empty, or whitespace
            if (string.IsNullOrWhiteSpace(inputImage))
                return "/images/default.png";

            // Step 2A: Validate if input is a complete external URL
            // - Uri (Uniform Resource Identifier): Built-in .NET class to parse and validate URLs.
            // - Uri.TryCreate: Safely attempts to parse without throwing exceptions on invalid input.
            // - UriKind.Absolute: Ensures the URI is fully-qualified (e.g., 'https://site.com/photo.jpg') rather than relative ('/images/photo.jpg').
            // - Uri.UriSchemeHttp / Https: Constants ensuring the URI uses web protocols, rejecting local paths like 'file:///C:/'.
            if (Uri.TryCreate(inputImage, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return inputImage; // Persist valid external URL directly in database
            }

            // Step 2B: Validate if input is an existing local relative path in wwwroot (e.g., '/images/avatar4.jpg')
            // - Prevents local file paths from entering Base64 decoding, preserving existing wwwroot image references.
            if (inputImage.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                return inputImage; // Retain existing local file reference without processing Base64
            }

            // Step 3: Enforce maximum Base64 size limit (~4.1M chars = ~3 MB binary) to prevent DoS attacks
            const int maxBase64Length = 4_100_000;
            if (inputImage.Length > maxBase64Length)
            {
                return "/images/default.png";
            }

            try
            {
                // Default MIME fallback values
                string mimeType = "image/png";
                string extension = ".png";
                string base64Data = inputImage;

                // Step 4: Parse Data URI scheme (data:image/png;base64,...)
                // - Data URI embeds raw image data into plain text.
                // - Split(";base64,") separates MIME metadata (data:image/png) from actual Base64 payload.
                if (inputImage.Contains(";base64,"))
                {
                    var parts = inputImage.Split(";base64,");

                    // Extract MIME type by stripping 'data:' prefix
                    mimeType = parts[0].Replace("data:", "").ToLower();

                    // Extract raw Base64 string payload
                    base64Data = parts[1];

                    // Step 5: MIME Types & Switch Expressions mapping
                    // - MIME types identify web formats (image/jpeg, image/png, image/webp).
                    // - C# switch expression maps MIME types to safe extensions, blocking scripts (.php, .aspx, .exe).
                    extension = mimeType switch
                    {
                        "image/jpeg" => ".jpg",
                        "image/png" => ".png",
                        "image/webp" => ".webp",
                        _ => string.Empty // Unallowed or unsafe format
                    };

                    // Reject files with unallowed extensions
                    if (string.IsNullOrEmpty(extension))
                        return "/images/default.png";
                }

                // Step 6: Convert Base64 payload string into raw binary byte array
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // Step 7: Resolve physical 'wwwroot/images' directory path
                string rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(rootPath, "images");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Step 8: Generate unique GUID file name to prevent name collisions and Path Traversal
                string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Step 9: Asynchronously save binary bytes to physical disk
                await File.WriteAllBytesAsync(filePath, imageBytes, ct);

                return $"/images/{uniqueFileName}";
            }
            catch
            {
                // Fallback to default avatar if Base64 string is corrupted
                return "/images/default.png";
            }
        }

        /// <summary>
        /// Deletes local custom images from wwwroot/images while ignoring default assets and web URLs.
        /// </summary>
        public void DeleteImageFile(string? imageUrl)
        {
            // Do not delete fallback avatar or empty references
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.EndsWith("default.png"))
                return;

            // Do not attempt local disk deletion for external web URLs
            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return;

            // Resolve physical wwwroot path and strip leading slashes
            string rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string relativePath = imageUrl.TrimStart('/', '\\');
            string fullPath = Path.Combine(rootPath, relativePath);

            // Delete file from disk if present
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}