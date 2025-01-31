using Newtonsoft.Json;

namespace SBMirror.Models
{
    public class ConfigCountdown : ModuleConfigBase
    {
        public List<CountdownItem> countdowns { get; set; } = new List<CountdownItem>();

        public override bool IsValid()
        {
            return intervalInSeconds > 0 && countdowns.Count > 0;
        }
    }

    public class CountdownItem
    {
        public string name { get; set; } = "";
        public DateTime date { get; set; } = DateTime.MinValue;
        public int daysBeforeStart { get; set; } = -1;
        public bool showTime { get; set; } = true;
        public bool recurring { get; set; } = true;

        [JsonIgnore]
        public double YearsDiff
        {
            get
            {
                return ((DateTime.Now - date).TotalDays / 365);
            }
        }

        [JsonIgnore]
        public bool ShowCountdown
        {
            get
            {
                double daysTill = 0;
                if (recurring)
                {
                    if (date < DateTime.Now)
                    {
                        var newDate = date.AddYears((int)Math.Round(YearsDiff));
                        daysTill = (newDate - DateTime.Now).TotalDays;
                    }
                    else
                    {
                        daysTill = (date - DateTime.Now).TotalDays;
                    }
                }
                else
                {
                    daysTill = (date - DateTime.Now).TotalDays;
                }
                return (daysTill > 0) && (daysTill <= daysBeforeStart);
            }
        }

        [JsonIgnore]
        public TimeSpan TimeUntil
        {
            get
            {
                DateTime dateToCheck;
                if (date < DateTime.Now)
                {
                    dateToCheck = date.AddYears((int)YearsDiff);
                }
                else
                {
                    dateToCheck = date;
                }
                return (dateToCheck - DateTime.Now);
            }
        }
    }
}
