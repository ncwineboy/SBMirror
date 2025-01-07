namespace SBMirror.Models.Weather
{
    public class weathergovStations
    {
        public List<object>? context { get; set; }
        public string? type { get; set; }
        public List<Feature>? features { get; set; }
        public List<string>? observationStations { get; set; }
        public Pagination? pagination { get; set; }
    }

    public class Pagination
    {
        public string? next { get; set; }
    }

    public class Feature
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public stationGeometry? geometry { get; set; }
        public stationProperties? properties { get; set; }
    }

    public class stationGeometry
    {
        public string? type { get; set; }
        public List<float>? coordinates { get; set; }
    }

    public class stationProperties
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public Elevation? elevation { get; set; }
        public string? stationIdentifier { get; set; }
        public string? name { get; set; }
        public string? timeZone { get; set; }
        public string? forecast { get; set; }
        public string? county { get; set; }
        public string? fireWeatherZone { get; set; }
    }

    public class Elevation
    {
        public string? unitCode { get; set; }
        public float value { get; set; }
    }
}
