using SBMirror.Models;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    public class CountdownService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger _logger;

        private static int intervalInSeconds = 1;
        private readonly System.Timers.Timer timer;

        private List<Countdown> countdowns = new List<Countdown>
        {
            new Countdown
            {
                Name = "Robbie's Birthday",
                Date = new DateTime(1970, 8, 14, 0, 0, 0),
                DaysBeforeStart = 30,
                Recurring = true
            },
            new Countdown
            {
                Name = "Rosalie's Birthday",
                Date = new DateTime(1972, 2, 13, 0, 0, 0),
                DaysBeforeStart = 30,
                Recurring = true
            },
            new Countdown
            {
                Name = "Kelly's Birthday",
                Date = new DateTime(1974, 8, 5, 0, 0, 0),
                DaysBeforeStart = 30,
                Recurring = true
            },
            new Countdown
            {
                Name = "Christmas",
                Date = new DateTime(2025, 12, 25, 0, 0, 0),
                DaysBeforeStart = 90,
                Recurring = true
            },
            new Countdown
            {
                Name = "New Years Day",
                Date= new DateTime(2025, 1, 1, 0, 0, 0),
                DaysBeforeStart = 5,
                Recurring = true
            }
        };
        public List<Countdown> ActiveCountdowns = new List<Countdown>();

        public CountdownService(IHttpClientFactory factory, ILoggerFactory logger) 
        {
            _factory = factory;
            timer = new System.Timers.Timer(TimeSpan.FromSeconds(intervalInSeconds));
            timer.Elapsed += TimerTick;
            timer.Enabled = true;
            _logger = logger.CreateLogger(typeof(CountdownService));
        }

        public event Action<List<Countdown>>? CountdownsChanged;

        public void TimerTick(object? sender, EventArgs e)
        {
            ActiveCountdowns = GetCurrentCountdowns().Result;
            CountdownsChanged?.Invoke(ActiveCountdowns);
        }

        public async Task<List<Countdown>> GetCurrentCountdowns()
        {
            List<Countdown> returnval = new List<Countdown>();
            try
            {
                await Task.FromResult(countdowns.Where(x => x.ShowCountdown == true).ToList());
            }
            catch (AggregateException ae)
            {
                _logger.LogError(ae.InnerException?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }   
            return returnval;
        }

    }
}
