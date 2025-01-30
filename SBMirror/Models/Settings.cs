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
                                name = "Robbie's Birthday",
                                date = new DateTime(1970, 8, 14, 0, 0, 0),
                                daysBeforeStart = 30,
                                recurring = true,
                                showTime = false
                            },
                            new CountdownItem
                            {
                                name = "Rosalie's Birthday",
                                date = new DateTime(1972, 2, 13, 0, 0, 0),
                                daysBeforeStart = 30,
                                recurring = true,
                                showTime = false
                            },
                            new CountdownItem
                            {
                                name = "Kelly's Birthday",
                                date = new DateTime(1974, 8, 5, 0, 0, 0),
                                daysBeforeStart = 30,
                                recurring = true,
                                showTime = false
                            },
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
                        wsMacAddress = "24:D7:EB:CF:2D:48",
                        wsApplicationKey = "c023fe6784564f48b85cdc3c09e9065545b24483988348cdab4ed003294f6eaa",
                        wsApiKey = "04df461c6e074ce3928465ae6799b9c1c60d9dbe68b44c60bf5502ef5973d248",
                        latitude = 35.6538498,
                        longitude = -81.3666442
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
                               url = "https://rss.nytimes.com/services/xml/rss/nyt/US.xml"
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
                        Header = "Rogers Family Calendar",
                        NumberOfDaysToShow = 10,
                        Calendars = new List<CalendarItem>
                        {
                            new CalendarItem 
                            {
                                Name = "Google",
                                Url = "https://calendar.google.com/calendar/ical/o92ckc0pjomtpsub624bang644%40group.calendar.google.com/private-952b2a11c0a0349173d89ede2977e4da/basic.ics"
                            }
                        }
                    }
                }
            }
        };

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
