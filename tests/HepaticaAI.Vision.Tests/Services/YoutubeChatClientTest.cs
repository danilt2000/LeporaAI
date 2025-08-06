using HepaticaAI.Vision.Services;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;
using Xunit.Abstractions;

namespace HepaticaAI.Vision.Tests.Services
{
    public class YoutubeChatClientTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly YoutubeChatClient _sut;

        public YoutubeChatClientTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);

            _sut = automocker.CreateInstance<YoutubeChatClient>();
        }

        [Fact]
        public async Task ConnectTest()
        {
            await _sut.Connect();

            await Task.Delay(1300000000);
        }
        
        [Fact]
        public async Task ScheduleLivestreamTest()
        {
            var scheduledTimeUtc = DateTime.UtcNow.AddMinutes(1);

            var title = $"Test Stream {DateTime.UtcNow:HH:mm:ss}";
            var description = "Автоматически запланированный стрим (тест)";

            var livestreamId = await _sut.ScheduleLivestreamAsync(
                scheduledTimeUtc,
                title,
                description
            );

            Assert.False(string.IsNullOrWhiteSpace(livestreamId));
            _testOutputHelper.WriteLine($"Запланирован тестовый стрим с ID: {livestreamId}");
        }
    }
}
