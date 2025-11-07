namespace Abig2025.Helpers
{
    public static class HoraArgentina
    {
        private static readonly TimeZoneInfo ArgentinaZone;

        static HoraArgentina()
        {
            try
            {
                ArgentinaZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: crear una zona horaria manual para Argentina (UTC-3)
                ArgentinaZone = TimeZoneInfo.CreateCustomTimeZone(
                    "Argentina Standard Time",
                    new TimeSpan(-3, 0, 0),
                    "Argentina Time",
                    "Argentina Time");
            }
        }

        // HORA CORREGIDA - Versión robusta
        public static DateTime Now
        {
            get
            {
                try
                {
                    var utcNow = DateTime.UtcNow;
                    return TimeZoneInfo.ConvertTimeFromUtc(utcNow, ArgentinaZone);
                }
                catch (Exception ex)
                {

                    return DateTime.UtcNow.AddHours(-3);
                }
            }
        }


        //  FECHA DE HOY EN ARGENTINA
        public static DateTime Today
        {
            get
            {
                try
                {
                    return Now.Date;
                }
                catch (Exception ex)
                {
                    return DateTime.UtcNow.AddHours(-3).Date;
                }
            }
        }

        //  INICIO DEL DÍA EN ARGENTINA (00:00:00)
        public static DateTime StartOfToday
        {
            get
            {
                return Today;
            }
        }

        //  FIN DEL DÍA EN ARGENTINA (23:59:59.999)
        public static DateTime EndOfToday
        {
            get
            {
                return Today.AddDays(1).AddTicks(-1);
            }
        }


        // MÉTODO DE EXTENSIÓN CORREGIDO
        public static DateTime ToArgentinaTime(this DateTime dateTime)
        {
            try
            {
                // Determinar el tipo de fecha que recibimos
                switch (dateTime.Kind)
                {
                    case DateTimeKind.Utc:
                        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, ArgentinaZone);

                    case DateTimeKind.Local:
                        // Si el servidor está en UTC, la hora local ya es UTC
                        if (TimeZoneInfo.Local.BaseUtcOffset == TimeSpan.Zero)
                        {
                            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, ArgentinaZone);
                        }
                        // Si el servidor tiene otra zona horaria, convertir primero a UTC
                        return TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), ArgentinaZone);

                    case DateTimeKind.Unspecified:
                    default:
                        // Asumir que es UTC (caso más común en servidores)
                        return TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                            ArgentinaZone);
                }
            }
            catch (Exception ex)
            {
                // Fallback seguro
                Console.WriteLine($"Error en ToArgentinaTime: {ex.Message}");
                return dateTime.Kind == DateTimeKind.Utc ?
                    dateTime.AddHours(-3) : dateTime;
            }
        }

    }
}
