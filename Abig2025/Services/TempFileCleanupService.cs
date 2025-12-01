using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Abig2025.Services
{
    public class TempFileCleanupService : BackgroundService
    {
        private readonly ILogger<TempFileCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6);
        private readonly TimeSpan _maxTempFileAge = TimeSpan.FromDays(2);

        public TempFileCleanupService(
            ILogger<TempFileCleanupService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de limpieza de archivos temporales iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldFilesAsync();
                    await CleanupOldDraftsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante la limpieza de archivos temporales.");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupOldFilesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var tempPath = Path.Combine(environment.WebRootPath, "uploads/temp");

            if (!Directory.Exists(tempPath))
                return;

            var cutoffDate = DateTime.UtcNow.Subtract(_maxTempFileAge);
            var files = Directory.GetFiles(tempPath);

            int deletedCount = 0;
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffDate)
                {
                    try
                    {
                        fileInfo.Delete();
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo eliminar el archivo temporal: {FileName}", file);
                    }
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInformation("Eliminados {Count} archivos temporales antiguos.", deletedCount);
            }
        }

        private async Task CleanupOldDraftsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var draftService = scope.ServiceProvider.GetRequiredService<IDraftService>();

            try
            {
                await draftService.CleanOldDraftsAsync(_maxTempFileAge);
                _logger.LogInformation("Limpieza de borradores antiguos completada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar borradores antiguos.");
            }
        }
    }
}
