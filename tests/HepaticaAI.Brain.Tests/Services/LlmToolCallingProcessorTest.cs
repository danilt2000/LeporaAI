using HepaticaAI.Brain.Services;
using HepaticaAI.Core.Models.Messages;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
    public class LlmToolCallingProcessorTest
    {
        private readonly LlmToolCalling _sut;
        public string ApiKey { get; set; }
        public LlmToolCallingProcessorTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);

            _sut = automocker.CreateInstance<LlmToolCalling>();
        }

        [Fact]
        public async Task SummarizeToolCallAsyncTest()
        {
            List<MessageEntry> messages =
            [
                new MessageEntry("user", "привет"),
                new MessageEntry("assistant", "привет"),
                new MessageEntry("user", "как прошёл твой день?")
                //new MessageEntry("user", "кто сидит у меня на стриме и также скинь мне клипы стрима")
                //new MessageEntry("user", "прочитай мне мой CV файл что там написано в нём")
                //new MessageEntry("user", "прочитай мне мой CV.pdf файл что там написано")
            ];


            var result = await _sut.SummarizeToolCallAsync(messages);
            //var result = await _sut.SummarizeAsync("расскажи что написанно в моих файлах");

        }

    }
}
