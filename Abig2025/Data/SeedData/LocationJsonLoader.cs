using System.Text.Json;
using Abig2025.Data.SeedData.Dtos;
using Abig2025.Models.Location;

namespace Abig2025.Data.SeedData
{
    public static class LocationJsonLoader
    {
        public static async Task LoadAsync(AppDbContext context, IWebHostEnvironment env)
        {
            // Seguridad: solo cargar una vez
            if (context.Cities.Any())
                return;

            var path = Path.Combine(
                env.ContentRootPath,
                "Data",
                "SeedData",
                "Datasets",
                "cities.ar.json"
            );

            

            if (!File.Exists(path))
                throw new FileNotFoundException($"Dataset no encontrado: {path}");

            var json = await File.ReadAllTextAsync(path);

            var data = JsonSerializer.Deserialize<LocationJsonRoot>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (data == null || !data.Provinces.Any())
                throw new Exception("Dataset de ciudades inválido");

            var cities = new List<City>();

            var citySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var province in data.Provinces)
            {
                foreach (var city in province.Cities)
                {
                    var key = $"{province.ProvinceId}|{city.Name.Trim()}";

                    if (citySet.Contains(key))
                        continue;

                    citySet.Add(key);

                    cities.Add(new City
                    {
                        Name = city.Name.Trim(),
                        ProvinceId = province.ProvinceId,
                        isActive = true
                    });
                }
            }

            context.Cities.AddRange(cities);
            await context.SaveChangesAsync();
        }
    }
}
