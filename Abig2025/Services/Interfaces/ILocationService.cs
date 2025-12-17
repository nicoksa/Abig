using Abig2025.Models.Location;

namespace Abig2025.Services.Interfaces
{
    public interface ILocationService
    {
        Task<List<Province>> GetProvincesAsync();
        Task<List<City>> GetCitiesByProvinceAsync(int provinceId);
        Task<City?> GetCityByIdAsync(int cityId);
        Task<Province?> GetProvinceByIdAsync(int provinceId);
    }
}
