using System.Timers;
using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Logic;
using SBMirror.Models;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    public class AmbientWeatherService : MirrorModuleServiceBase<ConfigWeather>, IAmbientWeatherService, IDisposable
    {
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

        public event Action<Lastdata>? LastdataChanged;

        public override async Task TimerTick(object? sender, ElapsedEventArgs e)
        {
            var value = await ReadWeatherStationData();
            if (value != null && value.dateutc != 0)
            {
                current = value;
                LastdataChanged?.Invoke(current);
            }
        }

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
