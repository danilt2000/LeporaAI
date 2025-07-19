using HepaticaAI.Brain.Services;
using HepaticaAI.Core.Interfaces.Memory;
using HepaticaAI.Core.Models.Messages;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace HepaticaAI.Brain.Tests.Services
{
    public class KoboldCppLLMClientTest
    {
        private readonly KoboldCppLLMClient _sut;

        public KoboldCppLLMClientTest()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);
            automocker.Use<IMemory>(automocker.CreateInstance<AIPromptsMemory>());

            _sut = automocker.CreateInstance<KoboldCppLLMClient>();
        }

        [Fact]
        public void InitializeTest()//TODO DON'T FORGET DISABLE PARALLELISMS FOR TEST INVOKING
        {
            _sut.Initialize();

            _sut.Dispose();
        }

        [Fact]
        public async Task GenerateAsyncTest()
        {
            _sut.Initialize();

            await Task.Delay(13000);

            var result = await _sut.GenerateAsync("User", "Расскажи мне про погоду сегодня");

            _sut.Dispose();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GenerateAsyncListTest()
        {
            _sut.Initialize();

            await Task.Delay(13000);
            List<MessageEntry> messages =
                [new MessageEntry("Lena", "Что ты думаешь о русско украинском конфликте расскажи про него поподробнее с чего именно он начался?")];
            //[new MessageEntry("Lena", "Что ты думаешь о русско украинском конфликте расскажи про него подробнее?"), new MessageEntry("Danil", "Fuck you"), new MessageEntry("Danil", "Tell me about your car"), new MessageEntry("Rafaello", "Rafaello is so idiot")];

            var result = await _sut.GenerateAsync(messages);

            _sut.Dispose();

            Assert.NotNull(result);
        }
    }
}
