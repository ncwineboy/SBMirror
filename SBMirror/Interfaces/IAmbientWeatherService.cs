using SBMirror.Models.Weather;

namespace SBMirror.Interfaces
{
    public interface IAmbientWeatherService
    {
        Task<Lastdata> ReadWeatherStationData(string macAddress, string applicationKey, string apiKey);
    }
}
