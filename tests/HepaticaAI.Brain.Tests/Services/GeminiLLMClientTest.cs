using System.Net;
using System.Net.Http.Headers;
using System.Text;
using GenerativeAI;
using Google.Apis.Auth.OAuth2;
using HepaticaAI.Brain.Services;
using HepaticaAI.Brain.Services.Gemini;
using HepaticaAI.Core.Interfaces.Memory;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Newtonsoft.Json;
using Xunit;

namespace HepaticaAI.Brain.Tests.Services
{
    public class GeminiLLMClientTest
    {
        private readonly GeminiLLMClient _sut;
        public string ApiKey { get; set; }
        public GeminiLLMClientTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            var _config = builder.Build();

            var automocker = new AutoMocker();

            automocker.Use<IConfiguration>(_config);
            automocker.Use<IMemory>(automocker.CreateInstance<AIPromptsMemory>());

            var apiKey = _config["Gemini:ApiKey"];
            ApiKey = _config["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini:ApiKey not configured. Set env GEMINI_API_KEY or appsettings.integration.json");

            var model = new GenerativeModel(apiKey, "gemini-2.0-flash");

            automocker.Use<IGenerativeModel>(model);

            _sut = automocker.CreateInstance<GeminiLLMClient>();
        }

        [Fact]
        public async Task GenerateAsyncTest()
        {
            var result = await _sut.GenerateAsync("Hepatica", "You are testing, tell me word test");

            Assert.NotEmpty(result);
        }
    }
}
