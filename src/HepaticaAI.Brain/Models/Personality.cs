namespace HepaticaAI.Brain.Models
{
        internal class Personality
        {
                public int n { get; set; }
                public int max_context_length { get; set; }
                public int max_length { get; set; }
                public double rep_pen { get; set; }
                public double temperature { get; set; }
                public double top_p { get; set; }
                public int top_k { get; set; }
                public int top_a { get; set; }
                public int typical { get; set; }
                public int tfs { get; set; }
                public int rep_pen_range { get; set; }
                public double rep_pen_slope { get; set; }
                public List<int> sampler_order { get; set; } = null!;
                public string memory { get; set; } = null!;
                public bool trim_stop { get; set; }
                public string genkey { get; set; } = null!;
                public int min_p { get; set; }
                public int dynatemp_range { get; set; }
                public int dynatemp_exponent { get; set; }
                public int smoothing_factor { get; set; }
                public List<object> banned_tokens { get; set; } = null!;
                public bool render_special { get; set; }
                public bool logprobs { get; set; }
                public int presence_penalty { get; set; }
                public LogitBias logit_bias { get; set; } = null!;
                public string prompt { get; set; } = null!;
                public bool quiet { get; set; }
                public List<string> stop_sequence { get; set; } = null!;
                public bool use_default_badwordsids { get; set; }
                public bool bypass_eos { get; set; }
        }
}
