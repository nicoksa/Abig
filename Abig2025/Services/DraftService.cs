using System.Text.Json;
using Abig2025.Data;
using Abig2025.Models.DTO;
using Abig2025.Models.Properties;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Services
{
    public class DraftService : IDraftService
    {
        private readonly AppDbContext _context;

        public DraftService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PropertyDraft?> GetDraftAsync(Guid draftId)
        {
            return await _context.PropertyDrafts
                .FirstOrDefaultAsync(d => d.DraftId == draftId);
        }

        public async Task<PropertyDraft> CreateDraftAsync(int userId, PropertyTempData data)
        {
            var draft = new PropertyDraft
            {
                DraftId = Guid.NewGuid(),
                UserId = userId,
                JsonData = JsonSerializer.Serialize(data),
                CurrentStep = 1
            };

            _context.PropertyDrafts.Add(draft);
            await _context.SaveChangesAsync();
            return draft;
        }

        public async Task UpdateDraftAsync(Guid draftId, PropertyTempData data, int nextStep)
        {
            var draft = await GetDraftAsync(draftId);
            if (draft == null) return;

            draft.JsonData = JsonSerializer.Serialize(data);
            draft.CurrentStep = nextStep;
            draft.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteDraftAsync(Guid draftId)
        {
            var draft = await GetDraftAsync(draftId);
            if (draft == null) return;

            _context.PropertyDrafts.Remove(draft);
            await _context.SaveChangesAsync();
        }
    }
}

