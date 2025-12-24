using Abig2025.Models.DTO;
using Abig2025.Models.ViewModels;
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

        // SOLO video va en el ViewModel
        [BindProperty]
        public PostStep2ViewModel Step2 { get; set; } = new();

        //  EL QUE USA LA VISTA
        [BindProperty]
        public List<IFormFile> Imagenes { get; set; } = new();

        public List<string> UploadErrors { get; set; } = new();

        public PropertyTempData Data { get; set; } = new();

        /* ===========================
           GET
        =========================== */
        public async Task<IActionResult> OnGet()
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            Data = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            Step2.VideoUrl = Data.VideoUrl;

            return Page();
        }

        /* ===========================
           POST STEP 2
        =========================== */
        public async Task<IActionResult> OnPostAsync()
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            var existingData =
                JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            // =========================
            // VIDEO
            // =========================
            existingData.VideoUrl = Step2.VideoUrl;

            // =========================
            // IMÁGENES NUEVAS
            // =========================
            if (Imagenes.Any())
            {
                int total = existingData.TempImages.Count + Imagenes.Count;

                if (total > 20)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Máximo 20 imágenes"
                    );

                    Data = existingData;
                    return Page();
                }

                foreach (var img in Imagenes)
                {
                    var fileName =
                        await _tempFileService.SaveTempImageAsync(img);

                    existingData.TempImages.Add(new TempImageInfo
                    {
                        FileName = fileName,
                        OriginalName = img.FileName,
                        Size = img.Length,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }


            // 🔥GUARDAR SIEMPRE STEP 2
   
            await _draftService.UpdateDraftAsync(
                DraftId.Value,
                existingData,
                nextStep: 2
            );

            // AVANZAR
            return RedirectToPage(
                "/Post/PostStep3",
                new { draftId = DraftId.Value }
            );
        }

        /* ===========================
           ELIMINAR IMAGEN
        =========================== */
        public async Task<IActionResult> OnPostDeleteImage(string fileName)
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            var existingData =
                JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            var image =
                existingData.TempImages
                    .FirstOrDefault(x => x.FileName == fileName);

            if (image != null)
            {
                _tempFileService.DeleteTempImage(fileName);
                existingData.TempImages.Remove(image);

                await _draftService.UpdateDraftAsync(
                    DraftId.Value,
                    existingData,
                    nextStep: 2
                );
            }

            return RedirectToPage(new
            {
                draftId = DraftId.Value,
                preserveNewImages = true  // Nuevo parámetro para indicar que debe preservar
            });
        }

        /* ===========================
           BACK
        =========================== */

        public async Task<IActionResult> OnPostBackAsync()
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

 
            // GUARDAR VIDEO (si hay cambios)

            existingData.VideoUrl = Step2.VideoUrl;

            // GUARDAR NUEVAS IMÁGENES 

            if (Imagenes.Any())
            {
                int total = existingData.TempImages.Count + Imagenes.Count;

                if (total > 20)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Máximo 20 imágenes"
                    );

                    Data = existingData;
                    return Page();
                }

                foreach (var img in Imagenes)
                {
                    var fileName = await _tempFileService.SaveTempImageAsync(img);

                    existingData.TempImages.Add(new TempImageInfo
                    {
                        FileName = fileName,
                        OriginalName = img.FileName,
                        Size = img.Length,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            //  GUARDAR SIEMPRE ANTES DE RETROCEDER

            await _draftService.UpdateDraftAsync(
                DraftId.Value,
                existingData,
                nextStep: 1  // Nota: retrocedemos al paso 1
            );


            // RETROCEDER AL PASO 1

            return RedirectToPage(
                "/Post/Post",
                new { draftId = DraftId.Value }
            );
        }
    }
}