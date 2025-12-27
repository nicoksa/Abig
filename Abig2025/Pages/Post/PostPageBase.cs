using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Helpers;
using Abig2025.Services;
using Abig2025.Models.Properties;

namespace Abig2025.Pages.Post
{
    /// Clase base para todas las páginas del flujo de publicación
    /// Centraliza la lógica común de autenticación y validación de drafts

    [Authorize]
    public abstract class PostPageBase : PageModel
    {

        /// Obtiene el ID del usuario autenticado

        protected int? GetAuthenticatedUserId() => User.GetUserId();


        /// Obtiene el ID del usuario o lanza excepción si no está autenticado

        protected int GetAuthenticatedUserIdOrThrow() => User.GetUserIdOrThrow();

 
        /// Valida que el draft existe y pertenece al usuario autenticado
        /// Retorna (error, draft) donde error != null si hay problema

        protected async Task<(IActionResult error, PropertyDraft draft)>
            GetAndValidateDraftAsync(Guid? draftId, IDraftService draftService, ILogger logger)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
            {
                return (RedirectToPage("/Account/Login"), null);
            }

            if (!draftId.HasValue)
            {
                return (RedirectToPage("/Post/Post"), null);
            }

            var draft = await draftService.GetDraftAsync(draftId.Value);

            if (draft == null)
            {
                return (RedirectToPage("/Post/Post"), null);
            }

            // Verificar ownership
            if (draft.UserId != userId.Value)
            {
                logger.LogWarning(
                    "Usuario {UserId} intentó acceder al draft {DraftId} que pertenece a {OwnerId}",
                    userId, draftId, draft.UserId
                );
                return (RedirectToPage("/Post/Post"), null);
            }

            return (null, draft);
        }

  
        /// Redirecciona al login si el usuario no está autenticado
        /// Retorna null si está autenticado

        protected IActionResult RedirectToLoginIfNotAuthenticated()
        {
            return GetAuthenticatedUserId().HasValue
                ? null
                : RedirectToPage("/Account/Login");
        }
    }
}
