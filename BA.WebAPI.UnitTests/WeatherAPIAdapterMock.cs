using System.Threading.Tasks;
using BA.WebAPI.Model;

namespace BA.WebAPI.UnitTests
{
    public class WeatherAPIAdapterMock : IWeatherAPIAdapter
    {
#pragma warning disable CS1998
        public async Task<string> GetConditionsAsync(Coordinates c)
        {
            return "Weather conditions";
        }
    }
}
