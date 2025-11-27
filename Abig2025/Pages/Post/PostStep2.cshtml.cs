// PostStep2.cshtml.cs
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

        public PostStep2Model(IDraftService draftService)
        {
            _draftService = draftService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? DraftId { get; set; }

        [BindProperty]
        public PropertyTempData Data { get; set; } = new();


        public async Task<IActionResult> OnGet()
        {
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
                return Page();

            // Actualizar el draft con los datos del paso 2
            await _draftService.UpdateDraftAsync(DraftId!.Value, Data, nextStep: 2);

            return RedirectToPage("/Post/PostStep3", new { draftId = DraftId });
        }
    }
}
