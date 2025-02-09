using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async Task ConnectToVoiceChannelAsyncTest()
        {
            await _sut.ConnectToVoiceChannelAsync(1175432324603719774, 1214942854213009443);

            Debug.WriteLine("Test");
        }
    }
}
