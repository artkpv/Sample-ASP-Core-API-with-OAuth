using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BA.WebAPI.Model
{
    public class WeatherAPIAdapter : IWeatherAPIAdapter
    {
        private readonly ILogger<IWeatherAPIAdapter> logger;

        private readonly IHttpClientFactory httpCFactory;

        public WeatherAPIAdapter(ILogger<IWeatherAPIAdapter> logger, IHttpClientFactory httpCFactory)
        {
            this.logger = logger;
            this.httpCFactory = httpCFactory;
        }

        public async Task<string> GetConditionsAsync(Coordinates c)
        {
            HttpClient client = httpCFactory.CreateClient();
            const string OpenWeatherAPIAppId = "74d244456143def7fedb94dfc527b25e"; 
            string url = $"http://api.openweathermap.org/data/2.5/weather?lat={c.Latitude}&lon={c.Longitude}&APPID={OpenWeatherAPIAppId}";
            try 
            {
                HttpResponseMessage r = await client.GetAsync(url);
                r.EnsureSuccessStatusCode();
                string weatherConditions = await r.Content.ReadAsStringAsync();
                return weatherConditions;
            }  
            catch(HttpRequestException e)
            {
                logger.LogError(e, "HTTP request failed");	
            }

            return null;
        }
    }
}

