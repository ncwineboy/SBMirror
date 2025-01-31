using Newtonsoft.Json;

namespace SBMirror.Models
{
    public static class Settings
    {
        public static Config config { get; set; } = new Config
        {
            units = "imperial",
            modules = new List<Module>
            {
                new Module {
                    name = "Clock",
                    position = "top left",
                    header = null,
                    config = new ConfigClock
                    {
                        timeFormat = 12,
                        timezone = "America/New_York"
                    }
                },
                new Module
                {
                    name = "Countdown",
                    position = "top center",
                    header = null,
                    config = new ConfigCountdown
                    {
                        intervalInSeconds = 1,
                        countdowns = new List<CountdownItem>
                        {
                            new CountdownItem
                            {
                                name = "Christmas",
                                date = new DateTime(2025, 12, 25, 0, 0, 0),
                                daysBeforeStart = 90,
                                recurring = true
                            },
                            new CountdownItem
                            {
                                name = "New Years Day",
                                date = new DateTime(2025, 1, 1, 0, 0, 0),
                                daysBeforeStart = 5,
                                recurring = true
                            }
                        }
                    }
                },
                new Module
                {
                    name = "CurrentWeather",
                    position = "top right",
                    header = null,
                    config = new ConfigWeather
                    {
                        intervalInSeconds = 30,
                        daysToForecast = 10,
                        localWeatherStation = true,
                        wsType = "AWS",
                        wsMacAddress = "",
                        wsApplicationKey = "",
                        wsApiKey = "",
                        latitude = 0,
                        longitude = 0
                    }
                },
                new Module
                {
                    name = "NewsFeeds",
                    position = "bottom bar",
                    header = null,
                    config = new ConfigRSSFeed
                    {
                        intervalInSeconds = 900,
                        displayLengthInSeconds = 180,
                        feeds = new List<RSSFeed>
                        {
                           new RSSFeed
                           {
                               title = "New York Times",
                               url = "https://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml"
                           }
                        }
                    }
                },
                new Module
                {
                    name = "Calendar",
                    position = "top left",
                    header = null,
                    config = new ConfigCalendar
                    {
                    }
                }
            }
        };

        /// <summary>
        /// Loads the configuration from a file.
        /// 
        /// Parameters:
        ///   filename (string): The path to the configuration file.
        /// 
        /// Returns:
        ///   None
        /// </summary>
        public static void LoadConfig(string filename)
        {
            if (File.Exists(filename))
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filename)) ?? new Config();
            }
        }

        /// <summary>
        /// Saves the configuration to a file.
        /// 
        /// Parameters:
        ///   filename (string): The path to the configuration file.
        /// 
        /// Returns:
        ///   None
        /// </summary>
        public static void SaveConfig(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(config));
        }


        // Retrieves a configuration of type T for a given module name.
        // 
        // Parameters:
        //   moduleName (string): The name of the module to retrieve the configuration for.
        // 
        // Returns:
        //   T?: The configuration of type T if found, otherwise the default value of T.
        public static T? GetConfig<T>(string moduleName)
        {
            var configPart = config.modules.FirstOrDefault(x => x.name == moduleName)?.config;
            if (configPart == null)
            {
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(configPart));
            }
            catch (JsonException)
            {
                return default;
            }
        }
    }
}
