using System.Dynamic;

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
                    config = new
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
                    config = new
                    {
                    }
                },
                new Module
                {
                    name = "CurrentWeather",
                    position = "top right",
                    header = null,
                    config = new 
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
                    name = "Photos",
                    position = "middle center",
                    header = null,
                    config = new
                    {
                    }
                }
            }
        };

        public static dynamic? GetConfig(string moduleName)
        {
            return config.modules.FirstOrDefault(x => x.name == moduleName)?.config;
        }
    }
}
