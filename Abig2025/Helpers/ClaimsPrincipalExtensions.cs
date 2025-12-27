using System.Security.Claims;

namespace Abig2025.Helpers
{

    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        public static int GetUserIdOrThrow(this ClaimsPrincipal principal)
        {
            return principal.GetUserId()
                ?? throw new UnauthorizedAccessException("Usuario no autenticado");
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            return principal?.Identity?.IsAuthenticated ?? false;
        }
    }
}