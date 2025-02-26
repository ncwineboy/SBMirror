using System.Timers;
using Ical.Net;
using Ical.Net.DataTypes;
using SBMirror.Logic;
using SBMirror.Models;

namespace SBMirror.Services
{
    /// <summary>
    /// Service to manage calendar events.
    /// </summary>
    public class CalendarService : MirrorModuleServiceBase<ConfigCalendar>
    {
        public List<CalendarEvent> CalendarEvents { get; private set; } = new List<CalendarEvent>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarService"/> class.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="logger"></param>
        public CalendarService(IHttpClientFactory factory, ILoggerFactory logger) : base(factory, logger, "Calendar")
        {
        }

        /// <summary>
        /// Timer tick event handler to fetch calendar events periodically.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override async Task TimerTick(object? sender, ElapsedEventArgs e)
        {
            CalendarEvents = await ReadCalendars();
            CalendarEventsChanged?.Invoke(CalendarEvents);
        }

        /// <summary>
        /// Calendar events changed
        /// </summary>
        public event Action<List<CalendarEvent>>? CalendarEventsChanged;

        /// <summary>
        /// Reads the calendars.
        /// </summary>
        /// <returns></returns>
        public async Task<List<CalendarEvent>> ReadCalendars()
        {
            List<CalendarEvent> returnval = new List<CalendarEvent>();
            if (_config != null && _config.IsValid())
            {
                switch (_config.CalendarType)
                {
                    case CalendarType.ICS:
                        returnval = await ReadICSCalendar();
                        break;
                    case CalendarType.Google:
                        break;
                }
            }
            return returnval.OrderBy(x => x.Start).ToList();
        }

        private async Task<List<CalendarEvent>> ReadICSCalendar()
        {
            List<CalendarEvent> returnval = new List<CalendarEvent>();
            var httpClient = GetClient();
            if (_config != null && _config.IsValid())
            {
                foreach (var calendar in _config.Calendars)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(calendar.Url);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var extract = ParseICS(json);
                            if (extract != null)
                            {
                                returnval.AddRange(extract);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Received a {response.StatusCode} from {calendar.Url}");
                        }
                    }
                    catch (AggregateException ae)
                    {
                        _logger.LogError(ae.InnerException?.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }
            }
            return returnval;
        }

        /// <summary>
        /// Parses the ICS.
        /// </summary>
        /// <param name="icsContent"></param>
        /// <returns></returns>
        public List<CalendarEvent> ParseICS(string icsContent)
        {
            var events = new List<CalendarEvent>();
            var calendar = Calendar.Load(icsContent);

            var startDate = new CalDateTime(DateTime.Now);
            var endDate = startDate.AddDays(_config.NumberOfDaysToShow);

            foreach (var eventItem in calendar.Events)
            {
                if (eventItem.RecurrenceRules == null ||
                    eventItem.RecurrenceRules.Count == 0)
                {
                    events.Add(new CalendarEvent
                    {
                        Summary = eventItem.Summary,
                        Start = DateTime.SpecifyKind(eventItem.Start.Value, DateTimeKind.Utc).ToLocalTime(),
                        End = DateTime.SpecifyKind(eventItem.End.Value, DateTimeKind.Utc).ToLocalTime()
                    });
                }
                else
                {
                    var recurranceDates = eventItem.GetOccurrences(DateTime.Now.Date, DateTime.Now.AddDays(_config.NumberOfDaysToShow));
                    if (recurranceDates.Count > 0)
                    {
                        foreach (var date in recurranceDates)
                        {

                            events.Add(new CalendarEvent
                            {
                                Summary = eventItem.Summary,
                                Start = date.Period.StartTime.Value,
                                End = date.Period.EndTime.Value
                            });
                        }
                    }

                }
            }


            return events.Where(x => (x.Start >= DateTime.Now.Date && x.AllDay) || (x.Start >= DateTime.Now)).ToList();
        }

    }
}
