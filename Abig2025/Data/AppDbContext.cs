// Data/AppDbContext.cs
using Abig2025.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Abig2025.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserReview> UserReviews{ get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar relación muchos a muchos entre Users y Roles
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            // Relación muchos a muchos User ↔ Property (favoritos, índice único)
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.PropertyId })
                .IsUnique();


            // Dni único en UserProfile
            modelBuilder.Entity<UserProfile>()
                .HasIndex(p => p.Dni)
                .IsUnique();

            // Relación User ↔ Property (dueño de propiedad)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Properties) 
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación UserReviews (evitar múltiples cascade paths)
            modelBuilder.Entity<UserReview>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserReview>()
                .HasOne(r => r.Reviewed)
                .WithMany()
                .HasForeignKey(r => r.ReviewedId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación User ↔ Favorites (evitar multiple cascade path)
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Property)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Insertar datos iniciales
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Administrator", Description = "Acceso completo al sistema" },
                new Role { RoleId = 2, RoleName = "User", Description = "Usuario estándar del sistema" },
                new Role { RoleId = 3, RoleName = "Guest", Description = "Usuario con permisos limitados" }
                );

            // Datos iniciales de Planes
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { PlanId = 1, Name = "Gratuito", Price = 0, DurationDays = 30, MaxPublications = 1, IncludesContractManagement = false },
                new SubscriptionPlan { PlanId = 2, Name = "Plata", Price = 1000, DurationDays = 45, MaxPublications = 4, IncludesContractManagement = false },
                new SubscriptionPlan { PlanId = 3, Name = "Oro", Price = 2000, DurationDays = 60, MaxPublications = -1, IncludesContractManagement = false },
                new SubscriptionPlan { PlanId = 4, Name = "Platino", Price = 2000, DurationDays = 60, MaxPublications = -1, IncludesContractManagement = true }
            );
        }
    }
}
