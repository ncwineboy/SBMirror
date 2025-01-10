using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    /// <summary>
    /// Service to interact with the National Weather Service (NWS) API and fetch weather forecasts.
    /// </summary>
    public class NationalWeatherService : INationalWeatherService, IDisposable
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger _logger;
        private static int daysToForecast = 7;
        private static int intervalInMinutes = 15;
        private readonly System.Timers.Timer timer;
        public WeatherForecast latestForecast { get; set; } = new WeatherForecast();

        /// <summary>
        /// Initializes a new instance of the <see cref="NationalWeatherService"/> class.
        /// </summary>
        /// <param name="factory">The HTTP client factory.</param>
        /// <param name="logger">The logger factory.</param>
        public NationalWeatherService(IHttpClientFactory factory, ILoggerFactory logger)
        {
            _factory = factory;
            timer = new System.Timers.Timer(TimeSpan.FromMinutes(intervalInMinutes));
            timer.Elapsed += TimerTick;
            timer.Enabled = true;
            _logger = logger.CreateLogger(typeof(NationalWeatherService));
        }

        /// <summary>
        /// Event triggered when the forecast changes.
        /// </summary>
        public event Action<WeatherForecast>? ForecastChanged;

        /// <summary>
        /// Timer tick event handler to fetch weather forecast periodically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void TimerTick(object? sender, EventArgs e)
        {
            latestForecast = GenerateWeatherForecast().Result;
            ForecastChanged?.Invoke(latestForecast);
        }

        /// <summary>
        /// Generates the weather forecast by fetching data from the NWS API.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the weather forecast.</returns>
        public async Task<WeatherForecast> GenerateWeatherForecast()
        {
            var returnval = new WeatherForecast();
            try
            {
                var value = await GetCurrentConditions(35.6538498, -81.3666442);
                if (!string.IsNullOrEmpty(value))
                {
                    returnval.currentConditions = value;
                }
                var forecast = await GetForecast(35.6538498, -81.3666442);
                if (forecast.properties != null && forecast.properties.periods != null)
                {
                    if (forecast.properties.periods.Count > 0)
                    {
                        for (int i = 0; i < daysToForecast; i++)
                        {
                            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                            var dayforecast = forecast.properties.periods.Where(x => DateOnly.FromDateTime(x.startTime) == date).ToList();
                            if (dayforecast.Count == 2)
                            {
                                returnval.forecast.Add(new WeatherByDay
                                {
                                    icon = dayforecast[0].icon,
                                    day = DateTime.Now.AddDays(i).DayOfWeek.ToString(),
                                    high = dayforecast[0].temperature,
                                    low = dayforecast[1].temperature,
                                    probprecip = dayforecast[0].probabilityOfPrecipitation?.value ?? 0
                                });
                            }
                            else
                            {
                                _logger.LogWarning($"Only have {dayforecast.Count} forecasts for {date}.");
                            }
                        }
                    }
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
            return returnval;
        }

        /// <summary>
        /// Gets the current weather conditions from the NWS API.
        /// </summary>
        /// <param name="latitude">The latitude of the location.</param>
        /// <param name="longitude">The longitude of the location.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the current weather conditions.</returns>
        private async Task<string> GetCurrentConditions(double latitude, double longitude)
        {
            var returnval = string.Empty;
            var points = await GetPoints(latitude, longitude);
            if (points != null && points.properties != null && points.properties.observationStations != null)
            {
                var stations = await GetStations(points.properties.observationStations);
                if (stations != null && stations.features != null && stations.features.First() != null)
                {
                    var url = $"{stations.features.First().id}/observations/latest";
                    var latest = await GetLatest(url);
                    if (latest != null && latest.properties != null)
                    {
                        returnval = latest.properties.icon;
                    }
                }
            }
            return returnval ?? string.Empty;
        }

        /// <summary>
        /// Gets the weather forecast from the NWS API.
        /// </summary>
        /// <param name="latitude">The latitude of the location.</param>
        /// <param name="longitude">The longitude of the location.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the weather forecast.</returns>
        private async Task<weathergovForecast> GetForecast(double latitude, double longitude)
        {
            var returnval = new weathergovForecast();
            var points = await GetPoints(latitude, longitude);
            var httpClient = _factory.CreateClient("www");
            if (points != null && points.properties != null)
            {
                var url = points.properties.forecast;
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    returnval = JsonConvert.DeserializeObject<weathergovForecast>(json) ?? new weathergovForecast();
                }
                else
                {
                    _logger.LogWarning($"Received a {response.StatusCode} from {url}");
                }
            }
            return returnval;
        }

        /// <summary>
        /// Gets the latest weather observation from the NWS API.
        /// </summary>
        /// <param name="url">The URL of the latest observation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the latest weather observation.</returns>
        private async Task<weathergovLatest> GetLatest(string url)
        {
            var returnval = new weathergovLatest();
            var httpClient = _factory.CreateClient("www");
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                returnval = JsonConvert.DeserializeObject<weathergovLatest>(json) ?? new weathergovLatest();
            }
            else
            {
                _logger.LogWarning($"Received a {response.StatusCode} from {url}");
            }
            return returnval;
        }

        /// <summary>
        /// Gets the points data from the NWS API.
        /// </summary>
        /// <param name="latitude">The latitude of the location.</param>
        /// <param name="longitude">The longitude of the location.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the points data.</returns>
        private async Task<weathergovPoints> GetPoints(double latitude, double longitude)
        {
            var returnval = new weathergovPoints();
            var httpClient = _factory.CreateClient("www");
            var url = $"https://api.weather.gov/points/{latitude},{longitude}";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                returnval = JsonConvert.DeserializeObject<weathergovPoints>(json) ?? new weathergovPoints();
            }
            else
            {
                _logger.LogWarning($"Received a {response.StatusCode} from {url}");
            }
            return returnval;
        }

        /// <summary>
        /// Gets the weather stations data from the NWS API.
        /// </summary>
        /// <param name="url">The URL of the weather stations data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the weather stations data.</returns>
        private async Task<weathergovStations> GetStations(string url)
        {
            var returnval = new weathergovStations();
            var httpClient = _factory.CreateClient("www");
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                returnval = JsonConvert.DeserializeObject<weathergovStations>(json) ?? new weathergovStations();
            }
            else
            {
                _logger.LogWarning($"Received a {response.StatusCode} from {url}");
            }
            return returnval;
        }

        /// <summary>
        /// Disposes the resources used by the <see cref="NationalWeatherService"/> class.
        /// </summary>
        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
