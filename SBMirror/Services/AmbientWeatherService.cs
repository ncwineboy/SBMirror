using System.Timers;
using Newtonsoft.Json;
using SBMirror.Interfaces;
using SBMirror.Models.Weather;

namespace SBMirror.Services
{
    public class AmbientWeatherService : IAmbientWeatherService, IDisposable
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger _logger;
        private static int intervalInSeconds = 15;
        private readonly System.Timers.Timer timer;
        public Lastdata current { get; set; } = new Lastdata();

        public AmbientWeatherService(IHttpClientFactory factory, ILoggerFactory logger)
        {
            _factory = factory;
            current = new Lastdata();
            timer = new System.Timers.Timer(TimeSpan.FromSeconds(intervalInSeconds));
            timer.Elapsed += TimerTick;
            timer.Enabled = true;
            _logger = logger.CreateLogger(typeof(AmbientWeatherService));
        }

        public event Action<Lastdata>? LastdataChanged;

        public void TimerTick(object? sender, ElapsedEventArgs e)
        {
            var value = ReadWeatherStationData("24:D7:EB:CF:2D:48", "c023fe6784564f48b85cdc3c09e9065545b24483988348cdab4ed003294f6eaa", "04df461c6e074ce3928465ae6799b9c1c60d9dbe68b44c60bf5502ef5973d248").Result;
            if (value.dateutc != 0)
            {
                current = value;
                LastdataChanged?.Invoke(current);
            }
        }

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

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
