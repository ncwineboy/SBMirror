namespace SBMirror.Models
{
    public class ConfigRSSFeed : ModuleConfigBase
    {
        public int displayLengthInSeconds { get; set; } = 180;

        public List<RSSFeed> feeds { get; set; } = new List<RSSFeed>();

        public override bool IsValid()
        {
            return intervalInSeconds > 0 && displayLengthInSeconds > 0 && feeds.Count > 0;
        }
    }

    public class RSSFeed
    {
        public string title { get; set; } = "";
        public string url { get; set; } = "";
    }

    public class RSSArticle
    {
        public string title { get; set; } = "";
        public string link { get; set; } = "";
        public string description { get; set; } = "";
        public DateTime pubDate { get; set; } = DateTime.MinValue;
    }
}
