using HepaticaAI.Vision.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Vision.Tests.Services
{
    public class YoutubeChatClientTest
    {
        private readonly YoutubeChatClient _sut;

        public YoutubeChatClientTest()
        {
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
            _sut.Connect();

            await Task.Delay(13000);
        }
    }
}
