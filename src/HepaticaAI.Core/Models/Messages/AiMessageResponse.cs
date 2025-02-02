namespace HepaticaAI.Core.Models.Messages
{
        public class AiMessageResponse
        {
                public string text { get; set; } = null!;
                public string finish_reason { get; set; } = null!;
                public object logprobs { get; set; } = null!;
                public int prompt_tokens { get; set; }
                public int completion_tokens { get; set; }
        }
}
