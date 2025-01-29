namespace HepaticaAI.Core.Interfaces.Voice
{
        public interface IVoiceSynthesis
        {
                void Initialize();//Todo if not used delete it from interface 

                byte[] SynthesizeSpeech(string text);//Todo if not used delete it from interface 

                void Speak(string text);//Todo if not used delete it from interface 
        }
}
