using HepaticaAI.Brain.Services;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
    //Link to project 
    //https://app.pinecone.io/organizations/-OVWbnM7c_4TBp-niA0Q/projects/63c6fb9a-22a6-4c31-9339-4581e22b8e07/indexes/lepora/browser
    public class PineconeKnowledgeBaseTest
    {
        private readonly PineconeKnowledgeBase _sut;
        public string ApiKey { get; set; }
        public PineconeKnowledgeBaseTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);

            _sut = automocker.CreateInstance<PineconeKnowledgeBase>();
        }

        [Fact]
        public async Task GetResponseTest()
        {
            await _sut.GetResponse();
        }
    }
}
