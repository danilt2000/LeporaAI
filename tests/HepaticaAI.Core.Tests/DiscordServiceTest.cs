using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Core.Tests
{
    public class DiscordServiceTest
    {
        private readonly DiscordService _sut;

        public DiscordServiceTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);

            _sut = automocker.CreateInstance<DiscordService>();
        }

        [Fact]
        public async Task GetUsernameByIdAsyncTest()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));

            var userName = await _sut.GetUsernameByIdAsync(293977705815343105);

            Assert.True(userName == "Hepatir");
        }
    }
}
