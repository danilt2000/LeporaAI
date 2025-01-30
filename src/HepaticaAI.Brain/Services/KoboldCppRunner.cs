using System.Diagnostics;

namespace HepaticaAI.Brain.Services
{
        public class KoboldCppRunner
        {
                public void StartKoboldCpp()
                {
                        //koboldcpp.exe --model "C:/Users/Danil/source/repos/HepaticaAI/src/HepaticaAI.Brain/Lama3.1-8B-EksiSozlukAI.Q5_K_M.gguf" --port 5001 --usevulkan 0 --gpulayers 35 --contextsize 4096 --draftamount 8 --draftgpulayers 999 --threads 7 --sdthreads 7 --skiplauncher
                        //Todo implement this call 
                        var process = new Process
                        {
                                StartInfo = new ProcessStartInfo
                                {
                                        FileName = "koboldcpp.exe",
                                        Arguments = "--config \"Lama31.kcppt\"",
                                        //RedirectStandardOutput = true,
                                        //RedirectStandardError = true,
                                        //UseShellExecute = true,
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                }
                        };

                        process.Start();
                }
        }
}
