using Abig2025.Data;
using Abig2025.Models.DTO;
using Abig2025.Models.Properties;
using Abig2025.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Abig2025.Helpers;


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

                    //  buscar los IDs
                    if (!string.IsNullOrEmpty(Data.Province) && !Data.ProvinceId.HasValue)
                    {
                        var province = await _context.Provinces
                            .FirstOrDefaultAsync(p => p.Name == Data.Province);
                        if (province != null) Data.ProvinceId = province.ProvinceId;
                    }

                    if (!string.IsNullOrEmpty(Data.City) && !Data.CityId.HasValue)
                    {
                        var city = await _context.Cities
                            .FirstOrDefaultAsync(c => c.Name == Data.City);
                        if (city != null) Data.CityId = city.CityId;
                    }

                    if (!string.IsNullOrEmpty(Data.Neighborhood) && !Data.NeighborhoodId.HasValue)
                    {
                        var neighborhood = await _context.Neighborhoods
                            .FirstOrDefaultAsync(n => n.Name == Data.Neighborhood);
                        if (neighborhood != null) Data.NeighborhoodId = neighborhood.NeighborhoodId;
                    }

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

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // ==========================
            // NUEVO DRAFT
            // ==========================
            if (!DraftId.HasValue)
            {
                var draft = new PropertyDraft
                {
                    DraftId = Guid.NewGuid(),
                    UserId = userId,
                    JsonData = JsonSerializer.Serialize(Data, jsonOptions),
                    CurrentStep = 1,
                    LastUpdated = DateTime.UtcNow
                };

                _context.PropertyDrafts.Add(draft);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Post/PostStep2", new { draftId = draft.DraftId });
            }

            // ==========================
            // DRAFT EXISTENTE (MERGE COMPLETO)
            // ==========================
            var existingDraft = await _draftService.GetDraftAsync(DraftId.Value);
            if (existingDraft == null)
            {
                return RedirectToPage("/Post/Post");
            }

            var existingData = JsonSerializer.Deserialize<PropertyTempData>(existingDraft.JsonData)!;

            // ACTUALIZAR TODOS LOS CAMPOS DEL STEP 1 CON LOS VALORES DEL FORMULARIO
            existingData.OperationType = Data.OperationType;
            existingData.PropertyType = Data.PropertyType;
            existingData.Subtype = Data.Subtype;
            existingData.Title = Data.Title;
            existingData.Description = Data.Description;
            existingData.Price = Data.Price;
            existingData.Currency = Data.Currency;
            existingData.Expenses = Data.Expenses;
            existingData.ExpensesCurrency = Data.ExpensesCurrency;

            // Ubicación 
            existingData.Province = Data.Province;
            existingData.City = Data.City;
            existingData.Neighborhood = Data.Neighborhood;
            existingData.ProvinceId = Data.ProvinceId;
            existingData.CityId = Data.CityId;
            existingData.NeighborhoodId = Data.NeighborhoodId;
            existingData.Street = Data.Street;
            existingData.Number = Data.Number;
            existingData.PostalCode = Data.PostalCode;

            // Características principales 
            existingData.MainRooms = Data.MainRooms;
            existingData.Bedrooms = Data.Bedrooms;
            existingData.Bathrooms = Data.Bathrooms;
            existingData.ParkingSpaces = Data.ParkingSpaces;
            existingData.CoveredArea = Data.CoveredArea;
            existingData.TotalArea = Data.TotalArea;
            existingData.Age = Data.Age;
            existingData.IsUnderConstruction = Data.IsUnderConstruction;
            existingData.IsNew = Data.IsNew;

            // Coordenadas 
            existingData.Latitude = Data.Latitude;
            existingData.Longitude = Data.Longitude;

            //  PRESERVAR DATOS DE OTROS PASOS (NO SOBREESCRIBIR)
            // Mantener features si ya existen
            if (existingData.Features == null || existingData.Features.Count == 0)
            {
                existingData.Features = Data.Features;
            }

            // Mantener imágenes temporales si ya existen
            if (existingData.TempImages == null || existingData.TempImages.Count == 0)
            {
                existingData.TempImages = Data.TempImages;
            }

            // Mantener video si ya existe
            if (string.IsNullOrEmpty(existingData.VideoUrl))
            {
                existingData.VideoUrl = Data.VideoUrl;
            }

            existingDraft.JsonData = JsonSerializer.Serialize(existingData, jsonOptions);
            existingDraft.CurrentStep = 1;
            existingDraft.LastUpdated = HoraArgentina.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Post/PostStep2", new { draftId = DraftId });
        }
    }
}

