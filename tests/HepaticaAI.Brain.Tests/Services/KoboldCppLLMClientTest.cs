using Castle.Core.Configuration;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HepaticaAI.Brain.Services;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
        public class KoboldCppLLMClientTest
        {
                private readonly KoboldCppLLMClient _sut;

                public KoboldCppLLMClientTest()
                {
                        var automocker = new AutoMocker();

                        _sut = automocker.CreateInstance<KoboldCppLLMClient>();
                }


                [Fact]
                public void InitializeTest()
                {
                        _sut.Initialize();
                }
        }
}
