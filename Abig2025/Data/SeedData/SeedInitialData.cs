// Data/SeedData/SeedInitialData.cs
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Data.SeedData
{
    public static class SeedInitialData
    {
        public static void SeedAll(ModelBuilder modelBuilder)
        {
            // Seed de roles, planes, etc. que ya tienes
            SeedRoles(modelBuilder);
            SeedSubscriptionPlans(modelBuilder);

            // Seed de ubicaciones
            SeedLocationData.Seed(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Abig2025.Models.Users.Role>().HasData(
                new { RoleId = 1, RoleName = "Administrator", Description = "Acceso completo al sistema" },
                new { RoleId = 2, RoleName = "User", Description = "Usuario estándar del sistema" },
                new { RoleId = 3, RoleName = "Guest", Description = "Usuario con permisos limitados" }
            );
        }

        private static void SeedSubscriptionPlans(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Abig2025.Models.Subscriptions.SubscriptionPlan>().HasData(
                new { PlanId = 1, Name = "Gratuito", Price = 0m, DurationDays = 30, MaxPublications = 1, IncludesContractManagement = false },
                new { PlanId = 2, Name = "Destacada", Price = 1000m, DurationDays = 30, MaxPublications = 3, IncludesContractManagement = false },
                new { PlanId = 3, Name = "Vip", Price = 2000m, DurationDays = 45, MaxPublications = -1, IncludesContractManagement = false }
            );
        }
    }
}