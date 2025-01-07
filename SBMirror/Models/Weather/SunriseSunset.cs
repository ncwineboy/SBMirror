namespace SBMirror.Models.Weather
{
    public class SunriseSunset
    {
        public DateTime SunriseUTC { get; set; }
        public DateTime Sunrise
        {
            get
            {
                return SunriseUTC.ToLocalTime();
            }
        }
        public DateTime SunsetUTC { get; set; }

        public DateTime Sunset
        {
            get
            {
                return SunsetUTC.ToLocalTime();
            }
        }
        public bool isDaytime { get; set; } = false;

        public bool Calculated { get; set; } = false;
    }
}
