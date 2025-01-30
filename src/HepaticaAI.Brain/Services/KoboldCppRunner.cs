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
                                        Arguments = "--model \"C:/Users/Danil/source/repos/HepaticaAI/src/HepaticaAI.Brain/Meta-Llama-3.1-8B-Instruct-Q6_K_L.gguf\" " +
                                                    "--port 5001 --usevulkan 0 --gpulayers 35 --contextsize 4096 " +
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
