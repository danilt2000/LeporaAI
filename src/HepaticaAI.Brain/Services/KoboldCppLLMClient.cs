using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HepaticaAI.Core.Interfaces.AI;

namespace HepaticaAI.Brain.Services
{
        internal class KoboldCppLLMClient : ILLMClient
        {
                public Task<string> GenerateAsync(string parametrs, string prompt)
                {
                        throw new NotImplementedException();
                }
        }
}
