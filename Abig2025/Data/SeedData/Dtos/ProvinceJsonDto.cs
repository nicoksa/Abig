namespace Abig2025.Data.SeedData.Dtos
{
    public class ProvinceJsonDto
    {
        public int ProvinceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public List<CityJsonDto> Cities { get; set; } = new();
    }
}
