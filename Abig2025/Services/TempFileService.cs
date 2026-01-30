using Abig2025.Services.Interfaces;


namespace Abig2025.Services
{

    public class TempFileService : ITempFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IImageConversionService _imageConversionService;
        private readonly ILogger<TempFileService> _logger;
        private const string TempFolder = "uploads/temp";
        private const string PermanentFolder = "uploads/properties";

        public TempFileService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IImageConversionService imageConversionService,
            ILogger<TempFileService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _imageConversionService = imageConversionService;
            _logger = logger;
        }

        // Método principal modificado para usar WebP
        public async Task<string> SaveTempImageAsync(IFormFile file)
        {
            return await SaveAndOptimizeImageAsync(file);
        }

        // Nuevo método para optimización explícita
        public async Task<string> SaveAndOptimizeImageAsync(IFormFile file, int quality = 80)
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

            // Generar nombre único con extensión .webp
            var originalName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeName = originalName.Length > 50 ? originalName.Substring(0, 50) : originalName;
            var fileName = $"{Guid.NewGuid()}_{safeName}.webp";
            var filePath = Path.Combine(tempPath, fileName);

            try
            {
                // Convertir y guardar como WebP
                await _imageConversionService.ConvertAndSaveAsync(file, filePath, quality);
                _logger.LogInformation("Imagen convertida a WebP: {FileName}, Calidad: {Quality}%",
                    fileName, quality);

                return fileName;
            }
            catch (Exception ex)
            {
                // Fallback: guardar el archivo original si falla la conversión
                _logger.LogWarning(ex, "Error al convertir a WebP, guardando original: {FileName}",
                    file.FileName);

                // Restaurar extensión original
                var originalExtension = Path.GetExtension(file.FileName);
                fileName = $"{Guid.NewGuid()}_{safeName}{originalExtension}";
                filePath = Path.Combine(tempPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return fileName;
            }
        }

        // Método para múltiples imágenes optimizadas
        public async Task<List<string>> SaveAndOptimizeImagesAsync(List<IFormFile> files, int quality = 80)
        {
            var fileNames = new List<string>();
            var tasks = new List<Task>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var fileName = await SaveAndOptimizeImageAsync(file, quality);
                        lock (fileNames)
                        {
                            fileNames.Add(fileName);
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
            return fileNames;
        }

        // Sobrecarga del método original para mantener compatibilidad
        public async Task<List<string>> SaveTempImagesAsync(List<IFormFile> files)
        {
            return await SaveAndOptimizeImagesAsync(files);
        }

        // Método para conversión directa a WebP con ruta específica
        public async Task<string> ConvertAndSaveAsWebPAsync(IFormFile file, string savePath, int quality = 80)
        {
            try
            {
                await _imageConversionService.ConvertAndSaveAsync(file, savePath, quality);
                _logger.LogInformation("Imagen convertida y guardada como WebP: {SavePath}", savePath);
                return savePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir y guardar como WebP: {SavePath}", savePath);
                throw;
            }
        }

        // Método para obtener stream WebP (útil para APIs o procesamiento en memoria)
        public async Task<Stream> ConvertToWebPStreamAsync(IFormFile file, int quality = 80)
        {
            try
            {
                return await _imageConversionService.ConvertToWebpAsync(file, quality);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir a stream WebP");
                throw;
            }
        }

        // Métodos existentes (sin cambios significativos)
        public string GetTempImagePath(string fileName)
        {
            return Path.Combine(_environment.WebRootPath, TempFolder, fileName);
        }

        public string GetTempImageUrl(string fileName)
        {
            return $"/{TempFolder}/{fileName}".Replace("\\", "/");
        }

        public void DeleteTempImage(string fileName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, TempFolder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Archivo temporal eliminado: {FileName}", fileName);
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
            var tempPath = GetTempImagePath(tempFileName);

            if (!File.Exists(tempPath))
            {
                throw new FileNotFoundException($"Archivo temporal no encontrado: {tempFileName}");
            }

            // Determinar extensión del archivo temporal
            var extension = Path.GetExtension(tempFileName).ToLowerInvariant();
            string permanentFileName;

            if (extension == ".webp")
            {
                // Ya es WebP, mantener el nombre
                permanentFileName = Path.GetFileName(tempFileName);
            }
            else
            {
                // Cambiar extensión a .webp
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(tempFileName);
                permanentFileName = $"{nameWithoutExtension}.webp";
            }

            var permanentFilePath = Path.Combine(permanentPath, permanentFileName);

            // Mover o copiar el archivo
            try
            {
                File.Move(tempPath, permanentFilePath);
                _logger.LogInformation(
                    "Imagen movida a permanente: {TempFileName} → {PermanentFileName}",
                    tempFileName, permanentFileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al mover archivo, intentando copiar");
                File.Copy(tempPath, permanentFilePath, true);
                File.Delete(tempPath);
            }

            return Path.Combine(propertyFolder, permanentFileName).Replace("\\", "/");
        }

        public async Task<List<string>> MoveAllToPermanentAsync(List<string> tempFileNames, int propertyId)
        {
            var permanentPaths = new List<string>();
            var tasks = new List<Task<string>>();

            foreach (var tempFileName in tempFileNames)
            {
                tasks.Add(MoveToPermanentAsync(tempFileName, propertyId));
            }

            permanentPaths.AddRange(await Task.WhenAll(tasks));
            return permanentPaths;
        }

        public bool IsValidImage(IFormFile file)
        {
            // Validar tamaño máximo (5MB)
            var maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
            {
                _logger.LogWarning("Archivo demasiado grande: {Size} bytes", file.Length);
                return false;
            }

            // Validar tipo de archivo
            var allowedExtensions = new[] {
                ".jpg", ".jpeg", ".png", ".gif", ".webp",
                ".bmp", ".tiff", ".tif"
            };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Extensión no permitida: {Extension}", extension);
                return false;
            }

            // Validar content type
            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/bmp",
                "image/tiff"
            };

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("Content-Type no permitido: {ContentType}", file.ContentType);
                return false;
            }

            return true;
        }
    }
}