using Abig2025.Models.Properties;
using Abig2025.Models.Properties.Enums;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Data.SeedData
{
    public static class FeatureDefinitionSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            int id = 1;

            FeatureDefinition F(
                string key,
                string display,
                string icon,
                FeatureScope scope,
                string group,
                int order
            ) => new FeatureDefinition
            {
                FeatureDefinitionId = id++,
                Key = key,
                DisplayName = display,
                Icon = icon,
                ValueType = FeatureValueType.Boolean,
                Scope = scope,
                Group = group,
                DisplayOrder = order,
                IsActive = true
            };

            modelBuilder.Entity<FeatureDefinition>().HasData(

                // =======================
                // MASCOTAS
                // =======================
                F("PermiteMascotas", "Permite mascotas", "bi bi-heart",
                    FeatureScope.All, "Mascotas", 1),

                // =======================
                // CARACTERÍSTICAS GENERALES
                // =======================
                F("AccesoDiscapacitados", "Acceso discapacitados", "bi bi-person-bounding-box",
                    FeatureScope.All, "Generales", 10),

                F("AptoProfesional", "Apto profesional", "bi bi-briefcase",
                    FeatureScope.All, "Generales", 11),

                F("UsoComercial", "Uso comercial", "bi bi-shop",
                    FeatureScope.All, "Generales", 12),

                // =======================
                // AMENITIES
                // =======================
                F("Parrilla", "Parrilla", "bi bi-fire",
                    FeatureScope.Urban, "Amenities", 20),

                F("Quincho", "Quincho", "bi bi-fire",
                    FeatureScope.Urban, "Amenities", 21),

                F("Gimnasio", "Gimnasio", "bi bi-activity",
                    FeatureScope.Urban, "Amenities", 22),

                F("Solarium", "Solarium", "bi bi-sun",
                    FeatureScope.Urban, "Amenities", 23),

                F("Hidromasaje", "Hidromasaje", "bi bi-droplet",
                    FeatureScope.Urban, "Amenities", 24),

                F("Sauna", "Sauna", "bi bi-thermometer-sun",
                    FeatureScope.Urban, "Amenities", 25),

                F("PistaDeportiva", "Pista deportiva", "bi bi-trophy",
                    FeatureScope.Urban, "Amenities", 26),

                F("SalaJuegos", "Sala de juegos", "bi bi-joystick",
                    FeatureScope.Urban, "Amenities", 27),

                // =======================
                // CONFORT / EQUIPAMIENTO
                // =======================
                F("AireAcondicionado", "Aire acondicionado", "bi bi-snow",
                    FeatureScope.All, "Confort", 30),

                F("Caldera", "Caldera", "bi bi-thermometer-high",
                    FeatureScope.All, "Confort", 31),

                F("Termotanque", "Termotanque", "bi bi-droplet-half",
                    FeatureScope.All, "Confort", 32),

                F("Lavavajillas", "Lavavajillas", "bi bi-cup-straw",
                    FeatureScope.All, "Confort", 33),

                F("Alarma", "Alarma", "bi bi-shield-lock",
                    FeatureScope.All, "Confort", 34),

                // =======================
                // INTERIORES
                // =======================
                F("Amueblado", "Amueblado", "bi bi-house-door",
                    FeatureScope.All, "Interiores", 40),

                F("CocinaEquipada", "Cocina equipada", "bi bi-egg-fried",
                    FeatureScope.All, "Interiores", 41),

                F("Lavadero", "Lavadero", "bi bi-bucket",
                    FeatureScope.All, "Interiores", 42),

                F("Baulera", "Baulera", "bi bi-box",
                    FeatureScope.All, "Interiores", 43),

                F("Toilette", "Toilette", "bi bi-door-open",
                    FeatureScope.All, "Interiores", 44),

                F("Balcon", "Balcón", "bi bi-layout-sidebar-inset",
                    FeatureScope.All, "Interiores", 45),

                F("Terraza", "Terraza", "bi bi-layers",
                    FeatureScope.All, "Interiores", 46),

                F("Patio", "Patio", "bi bi-tree",
                    FeatureScope.All, "Interiores", 47),

                F("Jardin", "Jardín", "bi bi-flower1",
                    FeatureScope.All, "Interiores", 48),

                // =======================
                // SERVICIOS
                // =======================
                F("InternetWifi", "Internet / WiFi", "bi bi-wifi",
                    FeatureScope.All, "Servicios", 50),

                F("Ascensor", "Ascensor", "bi bi-arrow-up",
                    FeatureScope.Urban, "Servicios", 51),

                F("Vigilancia", "Vigilancia", "bi bi-eye",
                    FeatureScope.All, "Servicios", 52),

                F("Limpieza", "Servicio de limpieza", "bi bi-stars",
                    FeatureScope.All, "Servicios", 53),

                F("Agua", "Agua", "bi bi-droplet",
                    FeatureScope.All, "Servicios", 54),

                F("Electricidad", "Electricidad", "bi bi-lightning",
                    FeatureScope.All, "Servicios", 55),

                // =======================
                // CAMPO
                // =======================
                F("Campo_Molino", "Molino", "bi bi-wind",
                    FeatureScope.Field, "Campo", 60),

                F("Campo_Manga", "Manga", "bi bi-diagram-3",
                    FeatureScope.Field, "Campo", 61),

                F("Campo_Cargador", "Cargador", "bi bi-truck",
                    FeatureScope.Field, "Campo", 62),

                F("Campo_Alambrado", "Alambrado", "bi bi-border",
                    FeatureScope.Field, "Campo", 63),

                F("Campo_Galpon", "Galpón", "bi bi-house",
                    FeatureScope.Field, "Campo", 64),

                F("Campo_SistemaRiego", "Sistema de riego", "bi bi-droplet",
                    FeatureScope.Field, "Campo", 65),

                F("Campo_Agricola", "Uso agrícola", "bi bi-seedling",
                    FeatureScope.Field, "Campo", 66),

                F("Campo_Ganadero", "Uso ganadero", "bi bi-cow",
                    FeatureScope.Field, "Campo", 67),

                F("Campo_Mixto", "Uso mixto", "bi bi-intersect",
                    FeatureScope.Field, "Campo", 68)
            );
        }
    }
}
