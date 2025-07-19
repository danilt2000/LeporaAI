using System.Diagnostics;
using HepaticaAI.Brain.Services;
using HepaticaAI.Core.Interfaces.Memory;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
    public class PythonWebSocketDiscordSpeechRecognitionTest
    {
        private PythonWebSocketDiscordSpeechRecognition _sut { get; set; }

        public PythonWebSocketDiscordSpeechRecognitionTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);
            automocker.Use<IMemory>(automocker.CreateInstance<AIPromptsMemory>());

            _sut = automocker.CreateInstance<PythonWebSocketDiscordSpeechRecognition>();
        }

        [Fact]
        public async Task SendMessageAsyncTest()
        {
            _sut.Start();

            await Task.Delay(13000);

            await _sut.SendMessageAsync("TEST TEST TEST TEST TEST TEST TEST TEST TEST TEST TEST TEST TEST TEST ");
            
            Debug.WriteLine("Test");
        }
    }
}
