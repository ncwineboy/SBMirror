using System.Timers;
using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Logic;
using SBMirror.Models;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    /// <summary>
    /// Service to interact with the National Weather Service (NWS) API and fetch weather forecasts.
    /// </summary>
    public class NationalWeatherService : MirrorModuleServiceBase<ConfigWeather>, INationalWeatherService, IDisposable
    {
        public WeatherForecast latestForecast { get; set; } = new WeatherForecast();

        /// <summary>
        /// Initializes a new instance of the <see cref="NationalWeatherService"/> class.
        /// </summary>
        /// <param name="factory">The HTTP client factory.</param>
        /// <param name="logger">The logger factory.</param>
        public NationalWeatherService(IHttpClientFactory factory, ILoggerFactory logger) : base(factory, logger, "CurrentWeather")
        {

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
        public override async Task TimerTick(object? sender, ElapsedEventArgs e)
        {
            latestForecast = await GenerateWeatherForecast();
            ForecastChanged?.Invoke(latestForecast);
        }

        /// <summary>
        /// Generates the weather forecast by fetching data from the NWS API.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the weather forecast.</returns>
        public async Task<WeatherForecast> GenerateWeatherForecast()
        {
            var returnval = new WeatherForecast();
            if (_config != null)
            {
                try
                {
                    var value = await GetCurrentConditions(_config.latitude, _config.longitude);
                    if (!string.IsNullOrEmpty(value))
                    {
                        returnval.currentConditions = value;
                    }
                    var forecast = await GetForecast(_config.latitude, _config.longitude);
                    if (forecast.properties != null && forecast.properties.periods != null)
                    {
                        if (forecast.properties.periods.Count > 0)
                        {
                            int availableForecast = forecast.properties.periods.Select(x => DateOnly.FromDateTime(x.startTime)).Distinct().Count();
                            int maxDays = (availableForecast < _config.daysToForecast) ? availableForecast : _config.daysToForecast;
                            for (int i = 0; i < maxDays; i++)
                            {
                                string dayofWeek = "";
                                if (i == 0)
                                {
                                    dayofWeek = "Today";
                                }
                                else if (i == 1)
                                {
                                    dayofWeek = "Tomorrow";
                                }
                                else
                                {
                                    dayofWeek = DateTime.Now.AddDays(i).DayOfWeek.ToString();
                                }
                                var date = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                                var dayforecast = forecast.properties.periods.Where(x => DateOnly.FromDateTime(x.startTime) == date).ToList();
                                if (dayforecast.Count == 2)
                                {
                                    returnval.forecast.Add(new WeatherByDay
                                    {
                                        icon = dayforecast[0].icon,
                                        day = dayofWeek,
                                        high = dayforecast[0].temperature,
                                        low = dayforecast[1].temperature,
                                        probprecip = dayforecast[0].probabilityOfPrecipitation?.value ?? 0
                                    });
                                }
                                else
                                {
                                    returnval.forecast.Add(new WeatherByDay
                                    {
                                        icon = dayforecast[0].icon,
                                        day = dayofWeek,
                                        high = dayforecast[0].temperature,
                                        low = dayforecast[0].temperature,
                                        probprecip = dayforecast[0].probabilityOfPrecipitation?.value ?? 0
                                    });
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
            var httpClient = GetClient();
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
            var httpClient = GetClient();
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
            var httpClient = GetClient();
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
            var httpClient = GetClient();
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
    }
}
