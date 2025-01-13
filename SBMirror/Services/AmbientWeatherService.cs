using System.Timers;
using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Models;
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

        private readonly System.Timers.Timer timer;
        public Lastdata current { get; set; } = new Lastdata();

        private WeatherConfig config = new WeatherConfig();


        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientWeatherService"/> class.
        /// </summary>
        /// <param name="factory">The HTTP client factory.</param>
        /// <param name="logger">The logger factory.</param>
        public AmbientWeatherService(IHttpClientFactory factory, ILoggerFactory logger)
        {
            _factory = factory;
            current = new Lastdata();

            _logger = logger.CreateLogger(typeof(AmbientWeatherService));

            ExtractConfig();

            timer = new System.Timers.Timer(TimeSpan.FromSeconds(config.intervalInSeconds));
            timer.Elapsed += TimerTick;
            timer.Enabled = true;
        }

        private void ExtractConfig()
        {
            var json = JsonConvert.SerializeObject(Settings.GetConfig("CurrentWeather") ?? new WeatherConfig());
            config = JsonConvert.DeserializeObject<WeatherConfig>(json) ?? new WeatherConfig();
            if (!config.IsValid())
            {
                _logger.LogWarning("Invalid configuration detected. Using default values.");
                config = new WeatherConfig();
            }
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
            var value = ReadWeatherStationData().Result;
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
        public async Task<Lastdata> ReadWeatherStationData()
        {
            var returnval = new Lastdata();
            if (!string.IsNullOrEmpty(config.wsApplicationKey) && !string.IsNullOrEmpty(config.wsApiKey))
            {
                var httpClient = _factory.CreateClient("www");
                var url = $"https://rt.ambientweather.net/v1/devices?applicationKey={config.wsApplicationKey}&apiKey={config.wsApiKey}";
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var observations = JsonConvert.DeserializeObject<List<AWSResponse>>(json) ?? new List<AWSResponse>();
                        if (observations.Count > 0)
                        {
                            AWSResponse? station = null;
                            if (!string.IsNullOrEmpty(config.wsMacAddress))
                            {
                                station = observations.Where(x => x.macAddress != null && x.macAddress.ToUpper() == config.wsMacAddress.ToUpper()).FirstOrDefault();
                            }
                            else
                            {
                                station = observations.FirstOrDefault();
                            }
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
