using Abig2025.Models.DTO;
using Abig2025.Models.ViewModels;
using Abig2025.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Abig2025.Services.Interfaces;

namespace Abig2025.Pages.Post
{
    public class PostStep2Model : PostPageBase
    {
        private readonly IDraftService _draftService;
        private readonly ITempFileService _tempFileService;
        private readonly ILogger<PostStep2Model> _logger;

        public PostStep2Model(
            IDraftService draftService,
            ITempFileService tempFileService,
            ILogger<PostStep2Model> logger)
        {
            _draftService = draftService;
            _tempFileService = tempFileService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? DraftId { get; set; }

        [BindProperty]
        public PostStep2ViewModel Step2 { get; set; } = new();

        [BindProperty]
        public List<IFormFile> Imagenes { get; set; } = new();

        // Nueva propiedad para recibir el orden de las imágenes
        [BindProperty]
        public string? ImageOrder { get; set; }

        public List<string> UploadErrors { get; set; } = new();

        public PropertyTempData Data { get; set; } = new();

        public async Task<IActionResult> OnGet()
        {
            var (error, draft) = await GetAndValidateDraftAsync(DraftId, _draftService, _logger);
            if (error != null) return error;

            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            if (draft == null)
                return RedirectToPage("/Post/Post");

            Data = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;
            Step2.VideoUrl = Data.VideoUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            // Guardar video
            existingData.VideoUrl = Step2.VideoUrl;

            // Validar límite de imágenes
            if (Imagenes.Any())
            {
                int total = existingData.TempImages.Count + Imagenes.Count;

                if (total > 20)
                {
                    ModelState.AddModelError(string.Empty, "Máximo 20 imágenes");
                    Data = existingData;
                    return Page();
                }
            }

            // Guardar las nuevas imágenes y aplicar orden en el mismo paso
            await SaveImagesWithOrder(existingData);

            // Guardar
            await _draftService.UpdateDraftAsync(DraftId.Value, existingData, nextStep: 2);

            return RedirectToPage("/Post/PostStep3", new { draftId = DraftId.Value });
        }

        public async Task<IActionResult> OnPostDeleteImage(string fileName)
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            var image = existingData.TempImages.FirstOrDefault(x => x.FileName == fileName);

            if (image != null)
            {
                _tempFileService.DeleteTempImage(fileName);
                existingData.TempImages.Remove(image);

                await _draftService.UpdateDraftAsync(DraftId.Value, existingData, nextStep: 2);
            }

            return RedirectToPage(new { draftId = DraftId.Value });
        }

        public async Task<IActionResult> OnPostBackAsync()
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            // Guardar video
            existingData.VideoUrl = Step2.VideoUrl;

            // Validar límite de imágenes
            if (Imagenes.Any())
            {
                int total = existingData.TempImages.Count + Imagenes.Count;

                if (total > 20)
                {
                    ModelState.AddModelError(string.Empty, "Máximo 20 imágenes");
                    Data = existingData;
                    return Page();
                }
            }

            // Guardar las nuevas imágenes y aplicar orden
            await SaveImagesWithOrder(existingData);

            // Guardar antes de retroceder
            await _draftService.UpdateDraftAsync(DraftId.Value, existingData, nextStep: 1);

            return RedirectToPage("/Post/Post", new { draftId = DraftId.Value });
        }

        private async Task SaveImagesWithOrder(PropertyTempData data)
        {
            // Si no hay nuevas imágenes y no hay orden, solo marcar la primera como principal
            if (!Imagenes.Any() && string.IsNullOrWhiteSpace(ImageOrder))
            {
                if (data.TempImages.Count > 0)
                {
                    foreach (var img in data.TempImages)
                    {
                        img.IsMain = false;
                    }
                    data.TempImages[0].IsMain = true;
                }
                return;
            }

            // Si no hay orden pero hay imágenes nuevas, agregarlas al final
            if (string.IsNullOrWhiteSpace(ImageOrder))
            {
                foreach (var img in Imagenes)
                {
                    var fileName = await _tempFileService.SaveTempImageAsync(img);

                    data.TempImages.Add(new TempImageInfo
                    {
                        FileName = fileName,
                        OriginalName = img.FileName,
                        Size = img.Length,
                        UploadedAt = DateTime.UtcNow
                    });
                }

                if (data.TempImages.Count > 0)
                {
                    foreach (var img in data.TempImages)
                    {
                        img.IsMain = false;
                    }
                    data.TempImages[0].IsMain = true;
                }
                return;
            }

            try
            {
                // Parsear el orden: "fileName1,CLIENT_NEW_0,fileName2,CLIENT_NEW_1"
                var orderItems = ImageOrder.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .ToList();

                _logger.LogInformation($"Orden recibido: {ImageOrder}");
                _logger.LogInformation($"Imágenes existentes: {data.TempImages.Count}");
                _logger.LogInformation($"Imágenes nuevas: {Imagenes.Count}");

                // Primero, guardar todas las imágenes nuevas en un diccionario temporal
                var newImagesByIndex = new Dictionary<int, TempImageInfo>();

                for (int i = 0; i < Imagenes.Count; i++)
                {
                    var img = Imagenes[i];
                    var fileName = await _tempFileService.SaveTempImageAsync(img);

                    newImagesByIndex[i] = new TempImageInfo
                    {
                        FileName = fileName,
                        OriginalName = img.FileName,
                        Size = img.Length,
                        UploadedAt = DateTime.UtcNow
                    };
                }

                // Crear la nueva lista ordenada
                var orderedImages = new List<TempImageInfo>();

                foreach (var orderItem in orderItems)
                {
                    if (orderItem.StartsWith("CLIENT_NEW_"))
                    {
                        // Es una imagen nueva del cliente
                        var indexStr = orderItem.Replace("CLIENT_NEW_", "");
                        if (int.TryParse(indexStr, out int clientIndex))
                        {
                            if (newImagesByIndex.ContainsKey(clientIndex))
                            {
                                orderedImages.Add(newImagesByIndex[clientIndex]);
                                _logger.LogInformation($"Agregando imagen nueva en posición: {orderedImages.Count - 1}");
                            }
                        }
                    }
                    else
                    {
                        // Es una imagen existente del servidor
                        var existingImage = data.TempImages.FirstOrDefault(img => img.FileName == orderItem);
                        if (existingImage != null)
                        {
                            orderedImages.Add(existingImage);
                            _logger.LogInformation($"Agregando imagen existente: {existingImage.FileName} en posición: {orderedImages.Count - 1}");
                        }
                    }
                }

                // Verificar que todas las imágenes estén incluidas
                var expectedTotal = data.TempImages.Count + Imagenes.Count;
                if (orderedImages.Count != expectedTotal)
                {
                    _logger.LogWarning($"Faltan imágenes en el orden. Esperadas: {expectedTotal}, Obtenidas: {orderedImages.Count}");

                    // Agregar imágenes faltantes al final
                    foreach (var existingImage in data.TempImages)
                    {
                        if (!orderedImages.Any(img => img.FileName == existingImage.FileName))
                        {
                            orderedImages.Add(existingImage);
                            _logger.LogInformation($"Agregando imagen faltante al final: {existingImage.FileName}");
                        }
                    }

                    foreach (var newImage in newImagesByIndex.Values)
                    {
                        if (!orderedImages.Any(img => img.FileName == newImage.FileName))
                        {
                            orderedImages.Add(newImage);
                            _logger.LogInformation($"Agregando imagen nueva faltante al final: {newImage.FileName}");
                        }
                    }
                }

                // Reemplazar la lista
                data.TempImages.Clear();
                data.TempImages.AddRange(orderedImages);

                // Marcar la primera como principal
                if (data.TempImages.Count > 0)
                {
                    foreach (var img in data.TempImages)
                    {
                        img.IsMain = false;
                    }
                    data.TempImages[0].IsMain = true;
                }

                _logger.LogInformation($"Orden aplicado correctamente. Total final: {data.TempImages.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar imágenes con orden");

                // Fallback: guardar las nuevas al final
                foreach (var img in Imagenes)
                {
                    var fileName = await _tempFileService.SaveTempImageAsync(img);

                    data.TempImages.Add(new TempImageInfo
                    {
                        FileName = fileName,
                        OriginalName = img.FileName,
                        Size = img.Length,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
        }

        private void ApplyImageOrder(PropertyTempData data)
        {
            if (string.IsNullOrWhiteSpace(ImageOrder) || data.TempImages.Count == 0)
            {
                // Si no hay orden especificado, solo marcar la primera como principal
                if (data.TempImages.Count > 0)
                {
                    foreach (var img in data.TempImages)
                    {
                        img.IsMain = false;
                    }
                    data.TempImages[0].IsMain = true;
                }
                return;
            }

            try
            {
                // El orden viene como: "fileName1,fileName2,fileName3"
                var orderedFileNames = ImageOrder.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(fn => fn.Trim())
                    .Where(fn => !fn.StartsWith("CLIENT_NEW_")) // Filtrar marcadores de cliente
                    .ToList();

                if (orderedFileNames.Count == 0)
                {
                    _logger.LogWarning("No hay imágenes guardadas en el orden");
                    return;
                }

                // Verificar que todas las imágenes en el orden existen
                var existingFileNames = data.TempImages.Select(img => img.FileName).ToHashSet();
                var validOrderedFileNames = orderedFileNames.Where(fn => existingFileNames.Contains(fn)).ToList();

                if (validOrderedFileNames.Count != data.TempImages.Count)
                {
                    _logger.LogWarning($"El número de imágenes en el orden ({validOrderedFileNames.Count}) no coincide con las imágenes guardadas ({data.TempImages.Count})");
                    // Agregar las imágenes que faltan en el orden
                    var missingImages = data.TempImages
                        .Where(img => !validOrderedFileNames.Contains(img.FileName))
                        .Select(img => img.FileName)
                        .ToList();
                    validOrderedFileNames.AddRange(missingImages);
                }

                // Crear nueva lista ordenada
                var orderedImages = new List<TempImageInfo>();

                foreach (var fileName in validOrderedFileNames)
                {
                    var image = data.TempImages.FirstOrDefault(img => img.FileName == fileName);
                    if (image != null)
                    {
                        orderedImages.Add(image);
                    }
                }

                // Reemplazar la lista
                data.TempImages.Clear();
                data.TempImages.AddRange(orderedImages);

                // Marcar la primera como principal
                if (data.TempImages.Count > 0)
                {
                    foreach (var img in data.TempImages)
                    {
                        img.IsMain = false;
                    }
                    data.TempImages[0].IsMain = true;
                }

                _logger.LogInformation($"Orden aplicado correctamente. Total de imágenes: {data.TempImages.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar el orden de las imágenes");
            }
        }
    }
}