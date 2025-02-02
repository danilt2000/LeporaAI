using HepaticaAI.Movement.Services;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Movement.Tests.Services
{
    public class VtubeStudioMovementTest
    {
        private readonly VtubeStudioMovement _sut;

        public VtubeStudioMovementTest()
        {
            var builder = new ConfigurationBuilder()
                    //.SetBasePath(Directory.GetCurrentDirectory())//Todo if json not used delete it 
                    //.AddJsonFile("appsettings.json", false, true)
                    ;

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);

            _sut = automocker.CreateInstance<VtubeStudioMovement>();
        }

        [Fact]
        public void StartIdleAnimationTest()
        {
            _sut.Initialize();

            _sut.StartIdleAnimation();

            Console.WriteLine("");
        }

        [Fact]
        public void SendRequestGetCurrentModelIdTest()
        {
            _sut.Initialize();

            _sut.SendRequestGetCurrentModelId();

            Console.WriteLine("");
        }
    }
}
