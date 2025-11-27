using Abig2025.Models.DTO;
using Abig2025.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Abig2025.Pages.Post
{
    public class PostModel : PageModel
    {
        private readonly IDraftService _draftService;

        public PostModel(IDraftService draftService)
        {
            _draftService = draftService;
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
            Console.WriteLine("OnPost ejecutado");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState no es válido");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return Page();
            }

            var userId = 1;

            if (!DraftId.HasValue)
            {
                var draft = await _draftService.CreateDraftAsync(userId, Data);
                return RedirectToPage("/Post/PostStep2", new { draftId = draft.DraftId });
            }

            await _draftService.UpdateDraftAsync(DraftId.Value, Data, nextStep: 1);
            return RedirectToPage("/Post/PostStep2", new { draftId = DraftId });
        }
    }
}

