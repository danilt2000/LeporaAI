using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Pinecone;
namespace HepaticaAI.Brain.Services
{
    public class PineconeKnowledgeBase//TODO ADD INTERFACE FOR IT 
    {//ALSO DON'T FORGET TO SAVE DATE OF MESSAGES
        private readonly IConfiguration _configuration;
        private readonly PineconeClient _pineconeClient;

        public PineconeKnowledgeBase(IConfiguration configuration)
        {
            _configuration = configuration;
            _pineconeClient = new PineconeClient(configuration["PineconeApiKey"]);
        }

        public async Task GetResponse()
        {
            var index = _pineconeClient.Index("lepora");

            await PerformTextSearch(index, "очень умная vtuber");
            await PerformIdSearch(index, "42");
        }

        private async Task PerformTextSearch(IndexClient index, string queryText)
        {
            var embedResp = await _pineconeClient.Inference.EmbedAsync(new EmbedRequest
            {
                Model = "llama-text-embed-v2",
                Inputs = new[]
                {
                    new EmbedRequestInputsItem { Text = "очень умная vtuber" }
                },
                Parameters = new Dictionary<string, object?>
                {
                    ["input_type"] = "query",
                    ["truncate"] = "END"
                }
            });

            float[] vector = embedResp.Data.First().AsDense().Values.ToArray();

            var resp = await index.QueryAsync(new QueryRequest
            {
                Vector = vector,
                TopK = 5,
                IncludeMetadata = true,
                Namespace = "__default__"
            });

            Debug.WriteLine($"=== Text Search (“{queryText}”) ===");
            foreach (var m in resp.Matches!)
                Debug.WriteLine($"{m.Id} ({m.Score:F4}): {m.Metadata?["text"]}");
        }

        private async Task PerformIdSearch(IndexClient index, string id)
        {
            var resp = await index.QueryAsync(new QueryRequest
            {
                Id = id,
                TopK = 5,
                IncludeMetadata = true,
                Namespace = "__default__"
            });

            Debug.WriteLine($"=== ID Search (“{id}”) ===");
            foreach (var m in resp.Matches!)
                Debug.WriteLine($"{m.Id} ({m.Score:F4}): {m.Metadata?["text"]}");
        }
    }
}
