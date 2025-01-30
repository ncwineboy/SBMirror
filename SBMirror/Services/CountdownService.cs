using System.Timers;
using SBMirror.Logic;
using SBMirror.Models;

namespace SBMirror.Services
{
    /// <summary>
    /// Service to manage countdowns for various events.
    /// </summary>
    public class CountdownService : MirrorModuleServiceBase<ConfigCountdown>
    {
        private List<CountdownItem> ActiveCountdowns = new List<CountdownItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CountdownService"/> class.
        /// </summary>
        /// <param name="factory">The HTTP client factory.</param>
        /// <param name="logger">The logger factory.</param>
        public CountdownService(IHttpClientFactory factory, ILoggerFactory logger) : base(factory, logger, "Countdown")
        {
        }

        /// <summary>
        /// Event triggered when the active countdowns change.
        /// </summary>
        public event Action<List<CountdownItem>>? CountdownsChanged;

        /// <summary>
        /// Timer tick event handler to update active countdowns periodically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/0>0 instance containing the event data.</param>
        public override async Task TimerTick(object? sender, ElapsedEventArgs e)
        {
            try
            {
                ActiveCountdowns = await GetCurrentCountdowns();
                CountdownsChanged?.Invoke(ActiveCountdowns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Gets the current active countdowns.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of active countdowns.</returns>
        public async Task<List<CountdownItem>> GetCurrentCountdowns()
        {
            List<CountdownItem> returnval = new List<CountdownItem>();
            if (_config != null)
            {
                try
                {
                    returnval = await Task.FromResult(_config.countdowns.Where(x => x.ShowCountdown == true).ToList());
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
    }
}
