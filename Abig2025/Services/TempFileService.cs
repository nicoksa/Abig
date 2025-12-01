using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Abig2025.Services
{
    public interface ITempFileService
    {
        Task<string> SaveTempImageAsync(IFormFile file);
        Task<List<string>> SaveTempImagesAsync(List<IFormFile> files);
        string GetTempImagePath(string fileName);
        void DeleteTempImage(string fileName);
        void DeleteTempImages(List<string> fileNames);
        Task<string> MoveToPermanentAsync(string tempFileName, int propertyId);
        Task<List<string>> MoveAllToPermanentAsync(List<string> tempFileNames, int propertyId);
        bool IsValidImage(IFormFile file);
    }

    public class TempFileService : ITempFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private const string TempFolder = "uploads/temp";
        private const string PermanentFolder = "uploads/properties";

        public TempFileService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<string> SaveTempImageAsync(IFormFile file)
        {
            if (!IsValidImage(file))
            {
                throw new ArgumentException("Archivo de imagen no válido");
            }

            // Crear carpeta temporal si no existe
            var tempPath = Path.Combine(_environment.WebRootPath, TempFolder);
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            // Generar nombre único
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(tempPath, fileName);

            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<List<string>> SaveTempImagesAsync(List<IFormFile> files)
        {
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = await SaveTempImageAsync(file);
                    fileNames.Add(fileName);
                }
            }

            return fileNames;
        }

        public string GetTempImagePath(string fileName)
        {
            return Path.Combine(TempFolder, fileName).Replace("\\", "/");
        }

        public void DeleteTempImage(string fileName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, TempFolder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteTempImages(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                DeleteTempImage(fileName);
            }
        }

        public async Task<string> MoveToPermanentAsync(string tempFileName, int propertyId)
        {
            // Crear carpeta de la propiedad
            var propertyFolder = Path.Combine(PermanentFolder, propertyId.ToString());
            var permanentPath = Path.Combine(_environment.WebRootPath, propertyFolder);

            if (!Directory.Exists(permanentPath))
            {
                Directory.CreateDirectory(permanentPath);
            }

            // Ruta del archivo temporal
            var tempPath = Path.Combine(_environment.WebRootPath, TempFolder, tempFileName);

            if (!File.Exists(tempPath))
            {
                throw new FileNotFoundException($"Archivo temporal no encontrado: {tempFileName}");
            }

            // Crear nombre único para el archivo permanente
            var extension = Path.GetExtension(tempFileName);
            var permanentFileName = $"{Guid.NewGuid()}{extension}";
            var permanentFilePath = Path.Combine(permanentPath, permanentFileName);

            // Mover el archivo
            File.Move(tempPath, permanentFilePath);

            return Path.Combine(propertyFolder, permanentFileName).Replace("\\", "/");
        }

        public async Task<List<string>> MoveAllToPermanentAsync(List<string> tempFileNames, int propertyId)
        {
            var permanentPaths = new List<string>();

            foreach (var tempFileName in tempFileNames)
            {
                var permanentPath = await MoveToPermanentAsync(tempFileName, propertyId);
                permanentPaths.Add(permanentPath);
            }

            return permanentPaths;
        }

        public bool IsValidImage(IFormFile file)
        {
            // Validar tamaño máximo (5MB)
            var maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
            {
                return false;
            }

            // Validar tipo de archivo
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return false;
            }

            // Validar content type
            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return false;
            }

            return true;
        }
    }
}

