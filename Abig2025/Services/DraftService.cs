using System.Text.Json;
using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.DTO;
using Abig2025.Models.Properties;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Services
{
    public class DraftService : IDraftService
    {
        private readonly AppDbContext _context;
        private readonly ITempFileService _tempFileService;

        public DraftService(AppDbContext context, ITempFileService tempFileService)
        {
            _context = context;
            _tempFileService = tempFileService;
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
            draft.LastUpdated = HoraArgentina.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteDraftAsync(Guid draftId)
        {
            var draft = await GetDraftAsync(draftId);
            if (draft == null) return;

            // Eliminar archivos temporales antes de borrar el draft
            var data = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData);
            if (data?.TempImages?.Count > 0)
            {
                var fileNames = data.TempImages.Select(img => img.FileName).ToList();
                _tempFileService.DeleteTempImages(fileNames);
            }

            _context.PropertyDrafts.Remove(draft);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserDraftsAsync(int userId)
        {
            var userDrafts = await _context.PropertyDrafts
                .Where(d => d.UserId == userId)
                .ToListAsync();

            foreach (var draft in userDrafts)
            {
                await DeleteDraftAsync(draft.DraftId);
            }
        }

        public async Task CleanOldDraftsAsync(TimeSpan olderThan)
        {
            var cutoffDate = HoraArgentina.Now.Subtract(olderThan);
            var oldDrafts = await _context.PropertyDrafts
                .Where(d => d.LastUpdated < cutoffDate)
                .ToListAsync();

            foreach (var draft in oldDrafts)
            {
                await DeleteDraftAsync(draft.DraftId);
            }
        }
    }
}

