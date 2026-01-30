namespace Abig2025.Services.Interfaces
{
    public interface ITempFileService
    {
        Task<string> SaveTempImageAsync(IFormFile file);
        Task<List<string>> SaveTempImagesAsync(List<IFormFile> files);
        string GetTempImagePath(string fileName);

        // Método para obtener URL 
        string GetTempImageUrl(string fileName);

        void DeleteTempImage(string fileName);
        void DeleteTempImages(List<string> fileNames);
        Task<string> MoveToPermanentAsync(string tempFileName, int propertyId);
        Task<List<string>> MoveAllToPermanentAsync(List<string> tempFileNames, int propertyId);
        bool IsValidImage(IFormFile file);

        // Métodos para optimización de imágenes
        Task<string> SaveAndOptimizeImageAsync(IFormFile file, int quality = 80);
        Task<List<string>> SaveAndOptimizeImagesAsync(List<IFormFile> files, int quality = 80);
        Task<string> ConvertAndSaveAsWebPAsync(IFormFile file, string savePath, int quality = 80);
        Task<Stream> ConvertToWebPStreamAsync(IFormFile file, int quality = 80);
    }
}
