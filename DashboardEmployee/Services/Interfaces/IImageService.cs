namespace DashboardEmployee.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(string? inputImage, CancellationToken ct = default);
        void DeleteImageFile(string? imageUrl);
    }
}