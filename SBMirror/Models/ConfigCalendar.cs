namespace SBMirror.Models
{
    public class ConfigCalendar : ModuleConfigBase
    {
        public string Header { get; set; } = string.Empty;
        public int NumberOfDaysToShow { get; set; }
        public List<CalendarItem> Calendars { get; set; } = new List<CalendarItem>();

        public override bool IsValid()
        {
            return NumberOfDaysToShow > 0 && Calendars.Count > 0;
        }
    }

    public class CalendarItem 
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
