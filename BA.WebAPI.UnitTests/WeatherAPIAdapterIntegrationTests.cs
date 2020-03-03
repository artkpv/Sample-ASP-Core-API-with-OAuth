using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using BA.WebAPI.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace BA.WebAPI.UnitTests
{
    public class WeatherAPIAdapterIntegrationTests
    {
        private readonly ITestOutputHelper output;

        private XunitLogger<IWeatherAPIAdapter> logger;

        public WeatherAPIAdapterIntegrationTests(ITestOutputHelper output)
        {
            this.output = output;
            logger = new XunitLogger<IWeatherAPIAdapter>(output);
        }

        [Fact]
        public async Task GetsWeatherConditions()
        {
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

            var service = new WeatherAPIAdapter(logger, factoryMock.Object);
            var saintPetersburgC = Coordinates.FromLatLong(59.9375, 30.308611);

            string answer = await service.GetConditionsAsync(saintPetersburgC);

            Assert.True(!string.IsNullOrWhiteSpace(answer));
        }
    }
}

