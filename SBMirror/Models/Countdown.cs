namespace SBMirror.Models
{
    public class Countdown
    {
        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public int DaysBeforeStart { get; set; }
        public bool Recurring { get; set; }

        public double YearsDiff {
            get
            {
                return ((DateTime.Now - Date).TotalDays / 365) + 1;
            }
        }

        public bool ShowCountdown
        {
            get
            {
                double daysTill = 0;
                if (Recurring)
                {
                    if (Date < DateTime.Now)
                    {
                        var newDate = Date.AddYears((int)YearsDiff);
                        daysTill = (newDate - DateTime.Now).TotalDays;
                    }
                    else
                    {
                        daysTill = (Date - DateTime.Now).TotalDays;
                    }
                }
                else
                {
                    daysTill = (Date - DateTime.Now).TotalDays;
                }
                return daysTill <= DaysBeforeStart;
            }
        }

        public TimeSpan TimeUntil
        {
            get
            {
                DateTime dateToCheck;
                if (Date < DateTime.Now)
                {
                    dateToCheck = Date.AddYears((int)YearsDiff);
                }
                else
                {
                    dateToCheck = Date;
                }
                return (dateToCheck - DateTime.Now);
            }
        }
    }
}
