namespace SBMirror.Models.Weather
{
    public class weathergovLatest
    {
        public object[]? context { get; set; }
        public string? id { get; set; }
        public string? type { get; set; }
        public latestGeometry? geometry { get; set; }
        public latestProperties? properties { get; set; }
    }

    public class latestGeometry
    {
        public string? type { get; set; }
        public float[]? coordinates { get; set; }
    }

    public class latestProperties
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public latestElevation? elevation { get; set; }
        public string? station { get; set; }
        public DateTime? timestamp { get; set; }
        public string? rawMessage { get; set; }
        public string? textDescription { get; set; }
        public string? icon { get; set; }
        public object[]? presentWeather { get; set; }
        public Temperature? temperature { get; set; }
        public Dewpoint? dewpoint { get; set; }
        public Winddirection? windDirection { get; set; }
        public Windspeed? windSpeed { get; set; }
        public Windgust? windGust { get; set; }
        public Barometricpressure? barometricPressure { get; set; }
        public Sealevelpressure? seaLevelPressure { get; set; }
        public Visibility? visibility { get; set; }
        public Maxtemperaturelast24hours? maxTemperatureLast24Hours { get; set; }
        public Mintemperaturelast24hours? minTemperatureLast24Hours { get; set; }
        public Precipitationlasthour? precipitationLastHour { get; set; }
        public Precipitationlast3hours? precipitationLast3Hours { get; set; }
        public Precipitationlast6hours? precipitationLast6Hours { get; set; }
        public Relativehumidity? relativeHumidity { get; set; }
        public Windchill? windChill { get; set; }
        public Heatindex? heatIndex { get; set; }
        public Cloudlayer[]? cloudLayers { get; set; }
    }

    public class latestElevation
    {
        public string? unitCode { get; set; }
        public int? value { get; set; }
    }

    public class Temperature
    {
        public string? unitCode { get; set; }
        public float? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Dewpoint
    {
        public string? unitCode { get; set; }
        public float? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Winddirection
    {
        public string? unitCode { get; set; }
        public int? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Windspeed
    {
        public string? unitCode { get; set; }
        public float? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Windgust
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Barometricpressure
    {
        public string? unitCode { get; set; }
        public int? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Sealevelpressure
    {
        public string? unitCode { get; set; }
        public int? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Visibility
    {
        public string? unitCode { get; set; }
        public int? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Maxtemperaturelast24hours
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
    }

    public class Mintemperaturelast24hours
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
    }

    public class Precipitationlasthour
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Precipitationlast3hours
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Precipitationlast6hours
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Relativehumidity
    {
        public string? unitCode { get; set; }
        public float? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Windchill
    {
        public string? unitCode { get; set; }
        public float? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Heatindex
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
        public string? qualityControl { get; set; }
    }

    public class Cloudlayer
    {
        public Base? _base { get; set; }
        public string? amount { get; set; }
    }

    public class Base
    {
        public string? unitCode { get; set; }
        public object? value { get; set; }
    }
}
