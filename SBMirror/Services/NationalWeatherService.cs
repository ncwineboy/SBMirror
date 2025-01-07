using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    public class NationalWeatherService : INationalWeatherService, IDisposable
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger _logger;
        private static int daysToForecast = 7;
        private static int intervalInMinutes = 15;
        private readonly System.Timers.Timer timer;
        public WeatherForecast latestForecast { get; set; } = new WeatherForecast();

        public NationalWeatherService(IHttpClientFactory factory, ILoggerFactory logger)
        {
            _factory = factory;
            timer = new System.Timers.Timer(TimeSpan.FromMinutes(intervalInMinutes));
            timer.Elapsed += TimerTick;
            timer.Enabled = true;
            _logger = logger.CreateLogger(typeof(NationalWeatherService));
        }

        public event Action<WeatherForecast>? ForecastChanged;

        public void TimerTick(object? sender, EventArgs e)
        {
            latestForecast = GenerateWeatherForecast().Result;
            ForecastChanged?.Invoke(latestForecast);
        }

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
                                    low = dayforecast[1].temperature
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

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
