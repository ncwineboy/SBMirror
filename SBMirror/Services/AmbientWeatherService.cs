using System.Timers;
using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    /// <summary>
    /// Service to interact with the Ambient Weather API and fetch weather data.
    /// </summary>
    public class AmbientWeatherService : IAmbientWeatherService, IDisposable
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger _logger;
        private static int intervalInSeconds = 15;
        private readonly System.Timers.Timer timer;
        public Lastdata current { get; set; } = new Lastdata();

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientWeatherService"/> class.
        /// </summary>
        /// <param name="factory">The HTTP client factory.</param>
        /// <param name="logger">The logger factory.</param>
        public AmbientWeatherService(IHttpClientFactory factory, ILoggerFactory logger)
        {
            _factory = factory;
            current = new Lastdata();
            timer = new System.Timers.Timer(TimeSpan.FromSeconds(intervalInSeconds));
            timer.Elapsed += TimerTick;
            timer.Enabled = true;
            _logger = logger.CreateLogger(typeof(AmbientWeatherService));
        }

        /// <summary>
        /// Event triggered when the last data changes.
        /// </summary>
        public event Action<Lastdata>? LastdataChanged;

        /// <summary>
        /// Timer tick event handler to fetch weather data periodically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        public void TimerTick(object? sender, ElapsedEventArgs e)
        {
            var value = ReadWeatherStationData("24:D7:EB:CF:2D:48", "c023fe6784564f48b85cdc3c09e9065545b24483988348cdab4ed003294f6eaa", "04df461c6e074ce3928465ae6799b9c1c60d9dbe68b44c60bf5502ef5973d248").Result;
            if (value.dateutc != 0)
            {
                current = value;
                LastdataChanged?.Invoke(current);
            }
        }

        /// <summary>
        /// Reads weather station data from the Ambient Weather API.
        /// </summary>
        /// <param name="macAddress">The MAC address of the weather station.</param>
        /// <param name="applicationKey">The application key for the API.</param>
        /// <param name="apiKey">The API key for the API.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the latest weather data.</returns>
        public async Task<Lastdata> ReadWeatherStationData(string macAddress, string applicationKey, string apiKey)
        {
            var returnval = new Lastdata();
            var httpClient = _factory.CreateClient("www");
            var url = $"https://rt.ambientweather.net/v1/devices?applicationKey={applicationKey}&apiKey={apiKey}";
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var observations = JsonConvert.DeserializeObject<List<AWSResponse>>(json) ?? new List<AWSResponse>();
                    if (observations.Count > 0)
                    {
                        var station = observations.Where(x => x.macAddress != null && x.macAddress.ToUpper() == macAddress.ToUpper()).FirstOrDefault();
                        if (station != null)
                        {
                            returnval = station.lastData;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Received a {response.StatusCode} from {url}");
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
            return returnval ?? new Lastdata();
        }

        /// <summary>
        /// Disposes the resources used by the <see cref="AmbientWeatherService"/> class.
        /// </summary>
        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
