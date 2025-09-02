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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar relación muchos a muchos entre Users y Roles
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            // Insertar datos iniciales
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Administrator", Description = "Acceso completo al sistema" },
                new Role { RoleId = 2, RoleName = "User", Description = "Usuario estándar del sistema" },
                new Role { RoleId = 3, RoleName = "Guest", Description = "Usuario con permisos limitados" }
            );
        }
    }
}
