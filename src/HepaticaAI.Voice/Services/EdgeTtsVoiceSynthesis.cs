using HepaticaAI.Core.Interfaces.Voice;
using NAudio.Wave;
using System.Diagnostics;

namespace HepaticaAI.Voice.Services
{
    internal class EdgeTtsVoiceSynthesis(IAudioPlayer audioPlayer) : IVoiceSynthesis
    {
        public string Voice { get; set; } = "ru-RU-SvetlanaNeural";
        public string Rate { get; set; } = "+18%";
        public string Pitch { get; set; } = "+11Hz";

        public void Speak(string text)
        {
            string outputFile = $"output{DateTime.Now:yyyyMMddHHmmss}.mp3";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "edge-tts",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add("--text");
            startInfo.ArgumentList.Add(text);
            startInfo.ArgumentList.Add("--voice");
            startInfo.ArgumentList.Add(Voice);
            startInfo.ArgumentList.Add("--rate");
            startInfo.ArgumentList.Add(Rate);
            startInfo.ArgumentList.Add("--pitch");
            startInfo.ArgumentList.Add(Pitch);
            startInfo.ArgumentList.Add("--write-media");
            startInfo.ArgumentList.Add(outputFile);

            try
            {
                using Process process = new Process { StartInfo = startInfo };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Debug.WriteLine($"Error: {error}");
                    return;
                }

                audioPlayer.PlayAudio(outputFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in process: " + ex.Message);
            }
        }
        
        public TimeSpan GetAudioDuration(string filePath)
        {
            using var reader = new Mp3FileReader(filePath);
            return reader.TotalTime;
        }
        
        public string GenerateSpeakAudioAndGetFilePath(string text)
        {
            string outputFile = $"output{DateTime.Now:yyyyMMddHHmmss}.mp3";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "edge-tts",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add("--text");
            startInfo.ArgumentList.Add(text);
            startInfo.ArgumentList.Add("--voice");
            startInfo.ArgumentList.Add(Voice);
            startInfo.ArgumentList.Add("--rate");
            startInfo.ArgumentList.Add(Rate);
            startInfo.ArgumentList.Add("--pitch");
            startInfo.ArgumentList.Add(Pitch);
            startInfo.ArgumentList.Add("--write-media");
            startInfo.ArgumentList.Add(outputFile);

            try
            {
                using Process process = new Process { StartInfo = startInfo };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Debug.WriteLine($"Error: {error}");
                    
                    throw new InvalidProgramException();
                }

                return outputFile;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in process: " + ex.Message);
                
                throw;
            }
        }
    }
}