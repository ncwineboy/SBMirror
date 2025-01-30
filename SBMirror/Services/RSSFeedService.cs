using System.Timers;
using System.Xml.Linq;
using SBMirror.Logic;
using SBMirror.Models;

namespace SBMirror.Services
{
    /// <summary>
    /// Service to fetch and manage RSS feed articles.
    /// </summary>
    public class RSSFeedService : MirrorModuleServiceBase<ConfigRSSFeed>, IDisposable
    {
        private readonly System.Timers.Timer? _displayTimer;

        /// <summary>   
        /// Gets the list of news articles.
        /// </summary>
        public List<RSSArticle> NewsArticles { get; private set; } = new List<RSSArticle>();

        /// <summary>
        /// Gets the current news article.
        /// </summary>
        public RSSArticle CurrentArticle { get; private set; } = new RSSArticle();

        /// <summary>
        /// Initializes a new instance of the <see cref="RSSFeedService"/> class.
        /// </summary>
        /// <param name="factory">The HTTP client factory.</param>
        /// <param name="logger">The logger factory.</param>
        public RSSFeedService(IHttpClientFactory factory, ILoggerFactory logger) : base(factory, logger, "NewsFeeds")
        {
            if (_config != null && _config.displayLengthInSeconds > 0)
            {
                _displayTimer = new System.Timers.Timer(TimeSpan.FromSeconds(_config.displayLengthInSeconds).TotalMilliseconds);
                _displayTimer.Elapsed += DisplayTimerTick;
                _displayTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Event triggered when the list of news articles changes.
        /// </summary>
        public event Action<List<RSSArticle>>? NewsArticlesChanged;

        /// <summary>
        /// Event triggered when the current news article changes.
        /// </summary>
        public event Action<RSSArticle>? CurrentArticleChanged;

        /// <summary>
        /// Timer tick event handler to fetch current news articles periodically.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayTimerTick(object? sender, ElapsedEventArgs e)
        {
            CurrentArticle = PickCurrentArticle();
            CurrentArticleChanged?.Invoke(CurrentArticle);
        }

        /// <summary>
        /// Timer tick event handler to fetch RSS feed articles periodically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        public override async Task TimerTick(object? sender, ElapsedEventArgs e)
        {
            try
            {
                NewsArticles = await LoadArticles();
                NewsArticlesChanged?.Invoke(NewsArticles);
            }
            catch (Exception ex)
            {
                // Handle the exception here
                // You can log the exception or display an error message to the user
                // For example:
                _logger.LogError($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Picks a random RSS article from the list of available news articles.
        /// </summary>
        /// <returns>A randomly selected RSS article, or a default article if no news articles are available.</returns>
        public RSSArticle PickCurrentArticle()
        {
            if (NewsArticles == null || NewsArticles.Count == 0)
            {
                return new RSSArticle();
            }

            var randomIndex = new Random().Next(0, NewsArticles.Count - 1);
            var returnval = NewsArticles[randomIndex];
            CurrentArticle = returnval;
            return returnval;
        }

        /// <summary>
        /// Loads the RSS feed articles from the configured feeds.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of RSS articles.</returns>
        public async Task<List<RSSArticle>> LoadArticles()
        {
            NewsArticles.Clear();
            List<RSSArticle> articles = new List<RSSArticle>();

            if (_config == null || _config.feeds == null)
            {
                _logger.LogWarning("No RSS feeds configured.");
                return articles;
            }

            foreach (var feed in _config.feeds)
            {
                try
                {
                    if (string.IsNullOrEmpty(feed.url))
                    {
                        _logger.LogWarning($"RSS feed {feed.title} has no URL.");
                        continue;
                    }

                    var client = GetClient();
                    var response = await client.GetAsync(feed.url);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var rss = XDocument.Parse(content);
                        var items = rss.Descendants("item");

                        foreach (var item in items)
                        {
                            var title = item.Element("title")?.Value;
                            var link = item.Element("link")?.Value;
                            var description = item.Element("description")?.Value;
                            var pubDate = item.Element("pubDate")?.Value;

                            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(pubDate))
                            {
                                try
                                {
                                    var article = new RSSArticle
                                    {
                                        title = title,
                                        link = link,
                                        description = description,
                                        pubDate = DateTime.Parse(pubDate)
                                    };
                                    articles.Add(article);
                                }
                                catch (FormatException ex)
                                {
                                    _logger.LogError(ex, $"Failed to parse pubDate for article {title}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Skipping article with missing fields: {title}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"Failed to load RSS feed {feed.title}. Status code: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException hre)
                {
                    _logger.LogError(hre, $"Error loading RSS feed {feed.title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error loading RSS feed {feed.title}");
                }
            }
            NewsArticles = articles;
            return articles;
        }
    }
}
