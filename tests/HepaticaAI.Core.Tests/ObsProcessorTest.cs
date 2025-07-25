using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Core.Tests
{
    public class ObsProcessorTest
    {
        private readonly ObsProcessor _sut;

        public ObsProcessorTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);

            _sut = automocker.CreateInstance<ObsProcessor>();
        }

        [Fact]
        public async Task GetUsernameByIdAsyncTest()
        {
            await _sut.ConnectAsync("ws://localhost:4455", "123456");

            await _sut.StartStreamAsync();

            await _sut.StopStreamAsync();

            await _sut.DisposeAsync();
        }
    }
}
