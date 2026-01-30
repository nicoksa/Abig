namespace Abig2025.Services.Interfaces
{
    public interface IImageConversionService
    {
        Task<Stream> ConvertToWebpAsync(IFormFile imageFile, int quality = 75);
        Task<string> ConvertAndSaveAsync(IFormFile imageFile, string savePath, int quality = 75);
    }
}
