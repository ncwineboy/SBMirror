namespace SBMirror.Models
{
    public class ConfigClock
    {
        public int timeFormat { get; set; } = 24;
        public string timezone { get; set; } = "America/New_York";
        public bool showSeconds { get; set;} = true;

        public bool IsValid()
        {
            return (timeFormat == 12 || timeFormat == 24) && !string.IsNullOrEmpty(timezone);
        }
    }
}
