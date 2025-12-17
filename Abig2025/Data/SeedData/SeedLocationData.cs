// Data/SeedData/SeedLocationData.cs
using Abig2025.Models.Location;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Data.SeedData
{
    public static class SeedLocationData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            SeedCountries(modelBuilder);
            SeedProvinces(modelBuilder);
            SeedCities(modelBuilder);
            SeedNeighborhoods(modelBuilder);
        }

        private static void SeedCountries(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>().HasData(
                new Country
                {
                    CountryId = 1,
                    Name = "Argentina",
                    Code = "AR"
                },
                // Puedes agregar más países si tu app es multi-país
                new Country
                {
                    CountryId = 2,
                    Name = "Uruguay",
                    Code = "UY"
                },
                new Country
                {
                    CountryId = 3,
                    Name = "Chile",
                    Code = "CL"
                }
            );
        }

        private static void SeedProvinces(ModelBuilder modelBuilder)
        {
            // Provincias de Argentina
            var provinces = new List<Province>
            {
                new() { ProvinceId = 1, Name = "Buenos Aires", Code = "BA", CountryId = 1 },
                new() { ProvinceId = 2, Name = "Catamarca", Code = "CA", CountryId = 1 },
                new() { ProvinceId = 3, Name = "Chaco", Code = "CH", CountryId = 1 },
                new() { ProvinceId = 4, Name = "Chubut", Code = "CT", CountryId = 1 },
                new() { ProvinceId = 5, Name = "Córdoba", Code = "CB", CountryId = 1 },
                new() { ProvinceId = 6, Name = "Corrientes", Code = "CR", CountryId = 1 },
                new() { ProvinceId = 7, Name = "Entre Ríos", Code = "ER", CountryId = 1 },
                new() { ProvinceId = 8, Name = "Formosa", Code = "FO", CountryId = 1 },
                new() { ProvinceId = 9, Name = "Jujuy", Code = "JY", CountryId = 1 },
                new() { ProvinceId = 10, Name = "La Pampa", Code = "LP", CountryId = 1 },
                new() { ProvinceId = 11, Name = "La Rioja", Code = "LR", CountryId = 1 },
                new() { ProvinceId = 12, Name = "Mendoza", Code = "MZ", CountryId = 1 },
                new() { ProvinceId = 13, Name = "Misiones", Code = "MI", CountryId = 1 },
                new() { ProvinceId = 14, Name = "Neuquén", Code = "NQ", CountryId = 1 },
                new() { ProvinceId = 15, Name = "Río Negro", Code = "RN", CountryId = 1 },
                new() { ProvinceId = 16, Name = "Salta", Code = "SA", CountryId = 1 },
                new() { ProvinceId = 17, Name = "San Juan", Code = "SJ", CountryId = 1 },
                new() { ProvinceId = 18, Name = "San Luis", Code = "SL", CountryId = 1 },
                new() { ProvinceId = 19, Name = "Santa Cruz", Code = "SC", CountryId = 1 },
                new() { ProvinceId = 20, Name = "Santa Fe", Code = "SF", CountryId = 1 },
                new() { ProvinceId = 21, Name = "Santiago del Estero", Code = "SE", CountryId = 1 },
                new() { ProvinceId = 22, Name = "Tierra del Fuego", Code = "TF", CountryId = 1 },
                new() { ProvinceId = 23, Name = "Tucumán", Code = "TM", CountryId = 1 }
            };

            modelBuilder.Entity<Province>().HasData(provinces);
        }

        private static void SeedCities(ModelBuilder modelBuilder)
        {
            // Ciudades principales de Buenos Aires (ejemplo)
            var cities = new List<City>
            {
                // Buenos Aires (ProvinceId = 1)
                new() { CityId = 100, Name = "Ciudad de Buenos Aires", ProvinceId = 1, PostalCodePrefix = "C1000" },
                
                // Mendoza (ProvinceId = 12)
                new() { CityId = 7, Name = "Mendoza Capital", ProvinceId = 12, PostalCodePrefix = "M5500" },
                new() { CityId = 8, Name = "Godoy Cruz", ProvinceId = 12, PostalCodePrefix = "M5501" },
                new() { CityId = 9, Name = "Guaymallén", ProvinceId = 12, PostalCodePrefix = "M5521" },
                new() { CityId = 10, Name = "Las Heras", ProvinceId = 12, PostalCodePrefix = "M5539" },
                
                // Córdoba (ProvinceId = 5)
                new() { CityId = 11, Name = "Córdoba Capital", ProvinceId = 5, PostalCodePrefix = "X5000" },
                new() { CityId = 12, Name = "Villa María", ProvinceId = 5, PostalCodePrefix = "X5900" },
                new() { CityId = 13, Name = "Río Cuarto", ProvinceId = 5, PostalCodePrefix = "X5800" },
                
                // Puedes agregar más ciudades importantes
            };
            modelBuilder.Entity<City>().HasData(cities);
        }


        private static void SeedNeighborhoods(ModelBuilder modelBuilder)
        {
            // Barrios de CABA
            var neighborhoods = new List<Neighborhood>
        {
            // CABA (CityId = 100)
            new() { NeighborhoodId = 1, Name = "Palermo", CityId = 100, PostalCodePrefix = "C1425" },
            new() { NeighborhoodId = 2, Name = "Recoleta", CityId = 100, PostalCodePrefix = "C1113" },
            new() { NeighborhoodId = 3, Name = "Belgrano", CityId = 100, PostalCodePrefix = "C1428" },
            new() { NeighborhoodId = 4, Name = "Núñez", CityId = 100, PostalCodePrefix = "C1429" },
            new() { NeighborhoodId = 5, Name = "Colegiales", CityId = 100, PostalCodePrefix = "C1426" },
            
            // San Miguel (CityId = 1)
            new() { NeighborhoodId = 101, Name = "Centro", CityId = 1, PostalCodePrefix = "B1663" },
            new() { NeighborhoodId = 102, Name = "Barrio Parque", CityId = 1, PostalCodePrefix = "B1663" },
            
            // Mendoza Capital (CityId = 7)
            new() { NeighborhoodId = 201, Name = "Centro", CityId = 7, PostalCodePrefix = "M5500" },
            new() { NeighborhoodId = 202, Name = "Quinta Sección", CityId = 7, PostalCodePrefix = "M5501" },
        };

            modelBuilder.Entity<Neighborhood>().HasData(neighborhoods);
        }
    }
}
