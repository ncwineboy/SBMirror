using SBMirror.Models.Weather;

namespace SBMirror.Interfaces
{
    public interface INationalWeatherService
    {
        Task<WeatherForecast> GenerateWeatherForecast();
    }
}
