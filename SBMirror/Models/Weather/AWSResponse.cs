namespace SBMirror.Models.Weather
{
    public class AWSResponse
    {
        public string? macAddress { get; set; }
        public Lastdata? lastData { get; set; }
        public Info? info { get; set; }
        public bool Success { get; set; } = false;
    }

    public class Lastdata
    {
        public long dateutc { get; set; }
        public float tempf { get; set; }
        public float humidity { get; set; }
        public float windspeedmph { get; set; }
        public float windgustmph { get; set; }
        public float maxdailygust { get; set; }
        public float winddir { get; set; }
        public string Direction
        {
            get
            {
                var val = (int)(winddir_avg10m / 22.5 + 0.5);
                string[] arr = ["N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"];
                return arr[val % 16];
            }
        }
        public float winddir_avg10m { get; set; }
        public int uv { get; set; }
        public float solarradiation { get; set; }
        public float hourlyrainin { get; set; }
        public float eventrainin { get; set; }
        public float dailyrainin { get; set; }
        public float weeklyrainin { get; set; }
        public float monthlyrainin { get; set; }
        public float yearlyrainin { get; set; }
        public int battout { get; set; }
        public float tempinf { get; set; }
        public float humidityin { get; set; }
        public float baromrelin { get; set; }
        public float baromabsin { get; set; }
        public int battin { get; set; }
        public float feelsLike { get; set; }
        public float dewPoint { get; set; }
        public float feelsLikein { get; set; }
        public float dewPointin { get; set; }
        public DateTime lastRain { get; set; }
        public string? tz { get; set; }
        public DateTime date { get; set; }
    }

    public class Info
    {
        public string? name { get; set; }
        public Coords? coords { get; set; }
    }

    public class Coords
    {
        public Coords1? coords { get; set; }
        public string? address { get; set; }
        public string? location { get; set; }
        public float elevation { get; set; }
        public Geo? geo { get; set; }
    }

    public class Coords1
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Geo
    {
        public string? type { get; set; }
        public float[]? coordinates { get; set; }
    }
}
