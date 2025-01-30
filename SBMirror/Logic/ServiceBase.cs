using System.Reflection;
using Newtonsoft.Json;
using SBMirror.Models;

namespace SBMirror.Logic
{
    /// <summary>
    /// Base class for services of the modules for the SBMirror application.
    /// </summary>
    /// <typeparam name="T">Model to be used for configuration settings</typeparam>
    public abstract class MirrorModuleServiceBase<T> : IDisposable
    {
        private bool disposedValue;
        public readonly IHttpClientFactory _factory;
        public readonly ILogger _logger;
        public readonly System.Timers.Timer? timer;
        public T? _config;

        public MirrorModuleServiceBase(IHttpClientFactory factory, ILoggerFactory logger, string? configName = null)
        {
            _factory = factory;
            _logger = logger.CreateLogger<MirrorModuleServiceBase<T>>();
            if (configName != null)
            {
                _config = Settings.GetConfig<T>(configName);

                if (_config != null)
                {
                    int? interval = (int?)GetPropertyValue<T>(_config, "intervalInSeconds");
                    if (interval != null && interval > 0)
                    {
                        timer = new System.Timers.Timer((int)interval * 1000);
                        timer.Elapsed += async (sender, e) => await TimerTick(sender, e);
                    }
                    else
                    {
                        timer = null;
                    }
                }
                else
                {
                    _logger.LogWarning($"No configuration named {configName} found.");
                }
            }
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <returns>HttpClient</returns>
        public HttpClient GetClient()
        {
            return _factory.CreateClient("www");
        }

        /// <summary>
        /// Deserializes the response.
        /// </summary>
        /// <typeparam name="T2">Model to deserialize response to</typeparam>
        /// <param name="response">HttpResponseMessage</param>
        /// <returns>T2</returns>
        public async Task<T2?> DeserializeAsync<T2>(HttpResponseMessage response)
        {
            if (response == null)
            {
                _logger.LogWarning("Response is null.");
            }
            else
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Receieved a {response.StatusCode} response.");
                }
                else
                {
                    try
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T2>(json);
                    }
                    catch (JsonSerializationException jsonEx)
                    {
                        _logger.LogWarning(jsonEx.Message);
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Executes the timer tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public abstract Task TimerTick(object? sender, System.Timers.ElapsedEventArgs e);

        private static object? GetPropertyValue<T3>(T3 instance, string propertyName)
        {
            // Use reflection to get the property
            PropertyInfo? propertyInfo = typeof(T3).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                return null;
            }

            // Get the property value from the instance
            return propertyInfo.GetValue(instance);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer?.Stop();
                    timer?.Dispose();
                }

                if (_config != null)
                {
                    if (_config is IDisposable)
                    {
                        ((IDisposable)_config).Dispose();
                    }
                    else
                    {
                        _config = default;
                    }
                }
                disposedValue = true;
            }
        }




        ~MirrorModuleServiceBase()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
