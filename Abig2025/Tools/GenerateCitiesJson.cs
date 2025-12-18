/*

using System.Text.Json;



var georefJson = File.ReadAllText("georef.localidades.json");
using var doc = JsonDocument.Parse(georefJson);

var provinces = new Dictionary<int, dynamic>();

foreach (var loc in doc.RootElement.GetProperty("localidades").EnumerateArray())
{
    var provinceName = loc.GetProperty("provincia").GetProperty("nombre").GetString();
    var cityName = loc.GetProperty("nombre").GetString();

    if (string.IsNullOrWhiteSpace(provinceName) || string.IsNullOrWhiteSpace(cityName))
        continue;

    var provinceId = provinceName switch
    {
        "Buenos Aires" => 1,
        "Catamarca" => 2,
        "Chaco" => 3,
        "Chubut" => 4,
        "Córdoba" => 5,
        "Corrientes" => 6,
        "Entre Ríos" => 7,
        "Formosa" => 8,
        "Jujuy" => 9,
        "La Pampa" => 10,
        "La Rioja" => 11,
        "Mendoza" => 12,
        "Misiones" => 13,
        "Neuquén" => 14,
        "Río Negro" => 15,
        "Salta" => 16,
        "San Juan" => 17,
        "San Luis" => 18,
        "Santa Cruz" => 19,
        "Santa Fe" => 20,
        "Santiago del Estero" => 21,
        "Tierra del Fuego" => 22,
        "Tucumán" => 23,
        _ => -1
    };

    if (provinceId == -1) continue;

    if (!provinces.ContainsKey(provinceId))
    {
        provinces[provinceId] = new
        {
            provinceId,
            cities = new List<object>()
        };
    }

    provinces[provinceId].cities.Add(new { name = cityName });
}

var output = new
{
    provinces = provinces.Values
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText("cities.ar.json", JsonSerializer.Serialize(output, options));

*/