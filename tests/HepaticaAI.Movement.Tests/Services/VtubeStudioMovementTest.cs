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
            var builder = new ConfigurationBuilder();

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);

            _sut = automocker.CreateInstance<VtubeStudioMovement>();
        }

        [Fact]
        public void StartIdleAnimationTest()//Todo think about deleting this test 
        {
            _sut.Initialize();

            _sut.StartIdleAnimation();
        }
    }
}
