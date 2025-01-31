using Newtonsoft.Json;

namespace SBMirror.Models
{
    public class CalendarEvent
    {
        public string? Summary { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Location { get; set; }

        [JsonIgnore]
        public int DaysFromNow
        {
            get
            {
                var now = DateTime.Now;
                return (Start - now).Days;
            }
        }

        [JsonIgnore]
        public bool AllDay
        {
            get
            {
                return Start.Hour == 0 && Start.Minute == 0 && End.Hour == 0 && End.Minute == 0;
            }
        }

        [JsonIgnore]
        public string TimeTill
        {
            get
            {
                var now = DateTime.Now;
                var diff = Start - now;
                if (AllDay)
                {
                    if (now.DayOfWeek > Start.DayOfWeek || diff.Days > 5)
                    {
                        return $"{Start:MMM} {Start.Day}";
                    }
                    else
                    {
                        return $"{Start.DayOfWeek}";
                    }
                }
                else
                {
                    if (DateOnly.FromDateTime(now) == DateOnly.FromDateTime(Start))
                    {
                        if (diff.Hours > 0)
                        {
                            if (diff.Minutes > 0)
                            {
                                float hours = (int)diff.Hours + ((int)diff.Minutes / 60);
                                return $"In {diff.TotalHours:0.0} hours";
                            }
                            else
                            {
                                return $"In {diff.Hours} hours";
                            }
                        }
                        else
                        {
                            return $"In {diff.Minutes} minutes";
                        }
                    }
                    else
                    {
                        if (now.DayOfWeek > Start.DayOfWeek || diff.Days > 5)
                        {
                            if (AllDay)
                            {
                                return $"{Start:MMM} {Start.Day}";
                            }
                            else
                            {
                                return $"{Start:MMM} {Start.Day} at {Start:h:mm tt}";
                            }
                        }
                        else
                        {
                            return $"{Start.DayOfWeek} at {Start:h:mm tt}";
                        }
                    }
                }
            }
        }
    }
}