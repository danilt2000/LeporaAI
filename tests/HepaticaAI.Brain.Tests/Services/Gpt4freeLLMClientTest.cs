using HepaticaAI.Brain.Services.Gpt4free;
using HepaticaAI.Core.Models.Messages;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
    public class Gpt4freeLLMClientTest
    {
        private readonly Gpt4freeLLMClient _sut;
        private readonly IConfiguration _configuration;
        public string ApiKey { get; set; }
        public Gpt4freeLLMClientTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);
            _configuration = _config;
            _sut = automocker.CreateInstance<Gpt4freeLLMClient>();
        }

        [Fact]
        public async Task GenerateAsyncTest()
        {
            List<MessageEntry> messages =
            [
                new MessageEntry("user", "иди нахуй")
            ];

            var result = await _sut.GenerateAsync(messages);

            Assert.NotEmpty(result);
        }
    }
}
