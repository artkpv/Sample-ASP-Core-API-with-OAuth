using System.Threading.Tasks;

namespace BA.WebAPI.Model
{
    public interface IWeatherAPIAdapter
    {
        Task<string> GetConditionsAsync(Coordinates c);
    }
}

