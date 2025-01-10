namespace SBMirror.Models.Weather
{
    public class WeatherForecast
    {
        public string currentConditions { get; set; } = "N/A";
        public List<WeatherByDay> forecast { get; set; } = new List<WeatherByDay>();
    }

    public class WeatherByDay
    {
        public string? icon { get; set; }
        public string? day { get; set; }
        public float high { get; set; }
        public float low { get; set; }
        public float probprecip { get; set; }

    }
}
