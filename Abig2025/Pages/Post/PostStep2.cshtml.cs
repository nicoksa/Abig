using Abig2025.Models.DTO;
using Abig2025.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Abig2025.Pages.Post
{
    public class PostStep2Model : PageModel
    {
        private readonly IDraftService _draftService;
        private readonly ITempFileService _tempFileService;

        public PostStep2Model(IDraftService draftService, ITempFileService tempFileService)
        {
            _draftService = draftService;
            _tempFileService = tempFileService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? DraftId { get; set; }

        [BindProperty]
        public PropertyTempData Data { get; set; } = new();

        [BindProperty]
        public List<IFormFile> Imagenes { get; set; } = new();

        public List<string> UploadErrors { get; set; } = new();

        public async Task<IActionResult> OnGet()
        {
            if (!DraftId.HasValue)
            {
                return RedirectToPage("/Post/Post");
            }

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
            {
                return RedirectToPage("/Post/Post");
            }

            Data = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!DraftId.HasValue)
            {
                return RedirectToPage("/Post/Post");
            }

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
            {
                return RedirectToPage("/Post/Post");
            }

            // Cargar los datos existentes
            var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            // Actualizar video
            if (!string.IsNullOrEmpty(Data.VideoUrl))
            {
                existingData.VideoUrl = Data.VideoUrl;
            }

            // Procesar nuevas imágenes
            if (Imagenes != null && Imagenes.Count > 0)
            {
                // Validar límite máximo de imágenes
                int totalImages = existingData.TempImages.Count + Imagenes.Count;
                if (totalImages > 20)
                {
                    ModelState.AddModelError("Imagenes", $"Solo puedes subir hasta 20 imágenes. Ya tienes {existingData.TempImages.Count}.");
                    Data = existingData;
                    return Page();
                }

                // Procesar cada imagen
                foreach (var imagen in Imagenes)
                {
                    if (imagen.Length > 0)
                    {
                        try
                        {
                            // Validar imagen
                            if (!_tempFileService.IsValidImage(imagen))
                            {
                                UploadErrors.Add($"La imagen '{imagen.FileName}' no es válida. Asegúrate de que sea JPG, PNG o GIF y pese menos de 5MB.");
                                continue;
                            }

                            // Guardar temporalmente
                            var fileName = await _tempFileService.SaveTempImageAsync(imagen);

                            // Agregar a la lista de imágenes temporales
                            existingData.TempImages.Add(new TempImageInfo
                            {
                                FileName = fileName,
                                OriginalName = imagen.FileName,
                                Size = imagen.Length
                            });
                        }
                        catch (Exception ex)
                        {
                            UploadErrors.Add($"Error al subir '{imagen.FileName}': {ex.Message}");
                        }
                    }
                }
            }

            // Actualizar el draft con los datos combinados
            await _draftService.UpdateDraftAsync(DraftId.Value, existingData, nextStep: 2);

            // Si hay errores, mostrar en la página
            if (UploadErrors.Count > 0)
            {
                foreach (var error in UploadErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                Data = existingData;
                return Page();
            }

            return RedirectToPage("/Post/PostStep3", new { draftId = DraftId });
        }

        // Método para eliminar una imagen específica
        public async Task<IActionResult> OnPostDeleteImage(string fileName)
        {
            if (!DraftId.HasValue)
            {
                return BadRequest();
            }

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
            {
                return BadRequest();
            }

            var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            // Eliminar la imagen de la lista
            var imageToRemove = existingData.TempImages.FirstOrDefault(img => img.FileName == fileName);
            if (imageToRemove != null)
            {
                // Eliminar archivo físico
                _tempFileService.DeleteTempImage(fileName);

                // Eliminar de la lista
                existingData.TempImages.Remove(imageToRemove);

                // Actualizar el draft
                await _draftService.UpdateDraftAsync(DraftId.Value, existingData, nextStep: 2);
            }

            return RedirectToPage(new { draftId = DraftId });
        }
    }
}
