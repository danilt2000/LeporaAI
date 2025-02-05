using System.Diagnostics;

namespace HepaticaAI.Brain.Services
{
        public class KoboldCppRunner
        {

                public void StartKoboldCpp()
                {
                        var process = new Process
                        {
                                StartInfo = new ProcessStartInfo
                                {
                                        FileName = "koboldcpp.exe",
                                    //Arguments = "--model \"C:/Users/Danil/source/repos/HepaticaAI/src/HepaticaAI.Brain/Meta-Llama-3.1-13B-Instruct-abliterated.i1-Q4_K_M.gguf\" " +
                                    //Arguments = "--model \"C:/Users/Danil/source/repos/HepaticaAI/src/HepaticaAI.Brain/Llama-3.1-13B-Instruct.Q4_K_M.gguf\" " +
                                                    Arguments = "--model \"C:/Users/Danil/source/repos/HepaticaAI/src/HepaticaAI.Brain/Meta-Llama-3-8B-Instruct.Q8_0.gguf\" " +
                                                    "--port 5001 --usevulkan 0 --gpulayers 35 --contextsize 16384 " +
                                                    //"--port 5001 --usevulkan 0 --gpulayers 35 --contextsize 16384 " +
                                                    "--draftamount 8 --draftgpulayers 999 --threads 7 --sdthreads 7 --skiplauncher",
                                        //RedirectStandardOutput = true,  
                                        //RedirectStandardError = true,   
                                        //UseShellExecute = StartConsole,        
                                        CreateNoWindow = true
                                }
                        };

                        process.Start();
                }
                public void StopKoboldCpp()
                {
                        var processes = Process.GetProcessesByName("koboldcpp");
                        foreach (var process in processes)
                        {
                                process.Kill();
                                process.WaitForExit();
                        }
                }
        }
}
