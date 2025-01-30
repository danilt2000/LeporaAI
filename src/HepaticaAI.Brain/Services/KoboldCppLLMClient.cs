using HepaticaAI.Core.Interfaces.AI;

namespace HepaticaAI.Brain.Services
{
        internal class KoboldCppLLMClient(KoboldCppRunner koboldCppRunner) : ILLMClient
        {
                public void Initialize()
                {
                        koboldCppRunner.StartKoboldCpp();
                }

                public Task<string> GenerateAsync(string parametrs, string prompt)
                {
                        return Task.FromResult(string.Empty);
                }

                public void Dispose()
                {
                        koboldCppRunner.StopKoboldCpp();
                }
        }
}
