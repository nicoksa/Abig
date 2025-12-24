// Data/AppDbContext.cs
using Abig2025.Models.Location;
using Abig2025.Models.Properties;
using Abig2025.Models.Subscriptions;
using Abig2025.Models.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using Abig2025.Data.SeedData;

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

        public DbSet<PropertyLocation> PropertyLocations { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyFeature> PropertyFeatures { get; set; }
        public DbSet<PropertyStatus> PropertyStatuses { get; set; }
        public DbSet<FeatureDefinition> FeatureDefinitions { get; set; }
        public DbSet<PropertyDraft> PropertyDrafts { get; set; }
        public DbSet<PropertyPublication> PropertyPublications { get; set; }


        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }

        


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
                .HasIndex(p => p.Dni);
                

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

            //Proopiedad y sus detalles

            modelBuilder.Entity<Property>()
            .HasOne(p => p.Location)
            .WithOne(l => l.Property)
            .HasForeignKey<PropertyLocation>(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Status)
                .WithOne(s => s.Property)
                .HasForeignKey<PropertyStatus>(s => s.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Property)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasMany(p => p.Features)
                .WithOne(f => f.Property)
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // CONFIGURACIÓN PARA PropertyPublication
            // ============================================

            modelBuilder.Entity<PropertyPublication>()
                .HasIndex(pp => new { pp.PropertyId, pp.UserId, pp.PlanId, pp.PublishedAt })
                .IsUnique();

            modelBuilder.Entity<PropertyPublication>()
                .HasOne(pp => pp.Property)
                .WithMany()
                .HasForeignKey(pp => pp.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PropertyPublication>()
                .HasOne(pp => pp.User)
                .WithMany()
                .HasForeignKey(pp => pp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertyPublication>()
                .HasOne(pp => pp.Plan)
                .WithMany()
                .HasForeignKey(pp => pp.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertyPublication>()
                .HasIndex(pp => pp.PublishedAt);

            modelBuilder.Entity<PropertyPublication>()
                .HasIndex(pp => pp.ExpiresAt);

            modelBuilder.Entity<PropertyPublication>()
                .HasIndex(pp => pp.IsActive);




            modelBuilder.Entity<Province>()
                .HasOne(p => p.Country)
                .WithMany(c => c.Provinces)
                .HasForeignKey(p => p.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<City>()
                .HasOne(c => c.Province)
                .WithMany(p => p.Cities)
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para búsqueda rápida
            modelBuilder.Entity<Province>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<City>()
                .HasIndex(c => new { c.Name, c.ProvinceId })
                .IsUnique();

            ConfigureNeighborhoodRelationships(modelBuilder);
            // SEED DATA 
            SeedInitialData.SeedAll(modelBuilder);

        }

        private void ConfigureNeighborhoodRelationships(ModelBuilder modelBuilder)
        {
            // Neighborhood ↔ City (muchos a uno)
            modelBuilder.Entity<Neighborhood>()
                .HasOne(n => n.City)
                .WithMany(c => c.Neighborhoods)
                .HasForeignKey(n => n.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único por nombre dentro de una ciudad
            modelBuilder.Entity<Neighborhood>()
                .HasIndex(n => new { n.Name, n.CityId })
                .IsUnique();
        }
    }
}
