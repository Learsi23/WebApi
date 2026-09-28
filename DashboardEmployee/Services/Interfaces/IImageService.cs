using static System.Net.Mime.MediaTypeNames;

namespace DashboardEmployee.Services.Interfaces
{
    public interface IImageService
    {

        /// <summary>Validates and stores an uploaded image. Returns its public URL, e.g. "/uploads/employees/{guid}.jpg".</summary>
        ///  <exception cref="Exceptions.BadRequestException">The file is empty, too large or not a JPEG/PNG/WebP image.</exception>

        Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default);

        ///<exception cref = "Exceptions.BadRequestException" > The file is empty, too large or not a JPEG/PNG/WebP image.</exception>
        void DeleteImageFile(string? imageUrl);
    }
}