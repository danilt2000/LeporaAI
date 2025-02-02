using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HepaticaAI.Vision.Services;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
        public class TwitchChatClientTest
        {
                private readonly TwitchChatClient _sut;

                public TwitchChatClientTest()
                {
                        var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", false, true);

                        var _config = builder.Build();

                        var automocker = new AutoMocker();

                        automocker.Use<IConfiguration>(_config);

                        _sut = automocker.CreateInstance<TwitchChatClient>();
                }

                [Fact]
                public async Task ConnectTest()
                {
                        _sut.Connect();
                        
                        await Task.Delay(13000);
                }
        }
}
