using System.Timers;
using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Logic;
using SBMirror.Models;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    /// <summary>
    /// Service to interact with the Ambient Weather API and fetch weather forecasts.
    /// </summary>
    public class AmbientWeatherService : MirrorModuleServiceBase<ConfigWeather>, IAmbientWeatherService, IDisposable
    {
        /// <summary>
        /// The current weather data.
        /// </summary>
        public Lastdata current { get; set; } = new Lastdata();

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientWeatherService"/> class.
        /// </summary>
        /// <param name="factory">The HTTP client factory.</param>
        /// <param name="logger">The logger factory.</param>
        public AmbientWeatherService(IHttpClientFactory factory, ILoggerFactory logger) : 
            base(factory, logger, "CurrentWeather") 
        {
            current = new Lastdata();
        }

        /// <summary>
        /// Event to notify subscribers when the current weather data changes.
        /// </summary>
        public event Action<Lastdata>? LastdataChanged;

        /// <summary>
        /// Timer tick event handler to fetch weather forecast periodically.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override async Task TimerTick(object? sender, ElapsedEventArgs e)
        {
            var value = await ReadWeatherStationData();
            if (value != null && value.dateutc != 0)
            {
                current = value;
                LastdataChanged?.Invoke(current);
            }
        }

        /// <summary>
        /// Reads the weather station data.
        /// </summary>
        /// <returns></returns>
        public async Task<Lastdata> ReadWeatherStationData()
        {
            if (_config != null)
            {
                if (string.IsNullOrEmpty(_config.wsApplicationKey) || string.IsNullOrEmpty(_config.wsApiKey))
                {
                    return new Lastdata();
                }

                var httpClient = GetClient();
                var url = $"https://rt.ambientweather.net/v1/devices?applicationKey={_config.wsApplicationKey}&apiKey={_config.wsApiKey}";

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
                            if (!string.IsNullOrEmpty(_config.wsMacAddress))
                            {
                                station = observations.Where(x => x.macAddress != null && x.macAddress.ToUpper() == _config.wsMacAddress.ToUpper()).FirstOrDefault();
                            }
                            else
                            {
                                station = observations.FirstOrDefault();
                            }

                            if (station != null && station.lastData != null)
                            {
                                return station.lastData;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Received a {response.StatusCode} from {url}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            return new Lastdata();
        }
    }
}
