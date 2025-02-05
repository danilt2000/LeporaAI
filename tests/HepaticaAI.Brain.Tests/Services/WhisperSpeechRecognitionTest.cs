using HepaticaAI.Brain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HepaticaAI.Core.Interfaces.Memory;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
    public class WhisperSpeechRecognitionTest
    {
        private readonly WhisperSpeechRecognition _sut;

        public WhisperSpeechRecognitionTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);
            automocker.Use<IMemory>(automocker.CreateInstance<AIPromptsMemory>());

            _sut = automocker.CreateInstance<WhisperSpeechRecognition>();
        }


        [Fact]
        public async Task RecognizeSpeechFromWavVideoTest()
        {

            var result = await _sut.RecognizeSpeechFromWavVideo();
            Assert.NotNull(result);
        }
    }
}
