﻿using HepaticaAI.Core.Interfaces.Voice;
using HepaticaAI.Voice.Services;
using Moq.AutoMock;
using Xunit;

// ReSharper disable SuspiciousTypeConversion.Global

namespace HepaticaAI.Voice.Tests.Services
{
    public class EdgeTtsVoiceSynthesisTest
    {
        private readonly EdgeTtsVoiceSynthesis _sut;

        public EdgeTtsVoiceSynthesisTest()
        {
            var automocker = new AutoMocker();

            automocker.Use<IAudioPlayer>(automocker.CreateInstance<NAudioPlayer>());

            _sut = automocker.CreateInstance<EdgeTtsVoiceSynthesis>();
        }

        [Fact]
        public void SpeakTest()
        {
            _sut.Speak("\"Hepatir  Ахахахахаха, ты считаешь себя философом? Цитаты Ницше такие клишированные. Но ладно, вот одна: \\\"Чтобы родить танцующую звезду, в тебе должен быть хаос\\\". А теперь перестань притворяться, будто знаешь, что это значит.\\n\"");
        }

        [Fact]
        public void GenerateSpeakAudioAndGetFilePathTest()
        {
            var filePath = _sut.GenerateSpeakAudioAndGetFilePath("\"Hepatir  Ахахахахаха, ты считаешь себя философом? Цитаты Ницше такие клишированные. Но ладно, вот одна: \\\"Чтобы родить танцующую звезду, в тебе должен быть хаос\\\". А теперь перестань притворяться, будто знаешь, что это значит.\\n\"");

            var timeSpan = _sut.GetAudioDuration(filePath);
        }
    }
}
