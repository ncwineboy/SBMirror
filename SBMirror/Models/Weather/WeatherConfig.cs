namespace SBMirror.Models.Weather
{
    public class WeatherConfig
    {
        private List<string> validWSTypes = new List<string>
        {
            "NONE",
            "AWS"
        };

        public int intervalInSeconds { get; set; } = 15;
        public int daysToForecast { get; set; } = 5;
        public bool localWeatherStation { get; set; } = false;
        public string wsType { get; set; } = "NONE";
        public string wsMacAddress { get; set; } = "";
        public string wsApplicationKey { get; set; } = "";
        public string wsApiKey { get; set; } = "";
        public double latitude { get; set; } = 51.477928;
        public double longitude { get; set; } = -0.001545;

        public bool IsValid()
        {
            return (intervalInSeconds > 0 && daysToForecast > 0 &&
                validWSTypes.Any(x => x == wsType.ToUpper()));
        }
    }
}
