using HepaticaAI.Brain.Services;
using HepaticaAI.Core.Interfaces.Memory;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
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
            automocker.Use<IMemory>(automocker.CreateInstance<AIPromptsMemory>());

            _sut = automocker.CreateInstance<DiscordService>();
        }

        [Fact]
        public async Task GetUsernameByIdAsyncTest()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            var userName  = await _sut.GetUsernameByIdAsync(293977705815343105);

            Assert.True(userName=="Hepatica");
        }
    }
}
