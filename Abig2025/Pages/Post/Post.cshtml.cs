using Abig2025.Data;
using Abig2025.Models.DTO;
using Abig2025.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Abig2025.Models.Properties;


namespace Abig2025.Pages.Post
{
    public class PostModel : PageModel
    {
        private readonly IDraftService _draftService;
        private readonly AppDbContext _context;

        public PostModel(IDraftService draftService, AppDbContext context)
        {
            _draftService = draftService;
            _context = context;
        }

        // --- Propiedades ---

        [BindProperty]
        public PropertyTempData Data { get; set; } = new();

        // El draftId viene opcional como parámetro en la URL
        [BindProperty(SupportsGet = true)]
        public Guid? DraftId { get; set; }

        public async Task<IActionResult> OnGet()
        {
            // Si estoy editando un borrador, cargo la data
            if (DraftId.HasValue)
            {
                var draft = await _draftService.GetDraftAsync(DraftId.Value);
                if (draft != null)
                {
                    Data = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = 1;

            // Configurar opciones de serialización para decimales
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            if (!DraftId.HasValue)
            {
                var draft = new PropertyDraft
                {
                    DraftId = Guid.NewGuid(),
                    UserId = userId,
                    JsonData = JsonSerializer.Serialize(Data, jsonOptions),
                    CurrentStep = 1
                };

                _context.PropertyDrafts.Add(draft);
                await _context.SaveChangesAsync();
                return RedirectToPage("/Post/PostStep2", new { draftId = draft.DraftId });
            }

            var existingDraft = await _draftService.GetDraftAsync(DraftId.Value);
            if (existingDraft != null)
            {
                existingDraft.JsonData = JsonSerializer.Serialize(Data, jsonOptions);
                existingDraft.CurrentStep = 1;
                existingDraft.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Post/PostStep2", new { draftId = DraftId });
        }
    }
}

