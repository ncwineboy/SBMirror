namespace SBMirror.Models
{
    public class ConfigCalendar : ModuleConfigBase
    {
        public string Header { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public CalendarType CalendarType
        {
            get
            {
                try
                {
                    return Enum.Parse<CalendarType>(Type);
                }
                catch
                {
                    return CalendarType.Invalid;
                }
            }
        }
        public int NumberOfDaysToShow { get; set; }
        public List<CalendarItem> Calendars { get; set; } = new List<CalendarItem>();

        public override bool IsValid()
        {
            return (!string.IsNullOrEmpty(Type) && CalendarType != CalendarType.Invalid) && NumberOfDaysToShow > 0 && Calendars.Count > 0;
        }
    }

    public class CalendarItem 
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
