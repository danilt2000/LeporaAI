using HepaticaAI.Core.Interfaces.Voice;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using TagLib;
using File = System.IO.File;
using Discord.Rest;

namespace HepaticaAI.Voice.Services
{
    internal class EdgeTtsVoiceSynthesis(IAudioPlayer audioPlayer, IConfiguration configuration) : IVoiceSynthesis
    {
        public string Voice { get; set; } = "ru-RU-SvetlanaNeural";
        //public string Voice { get; set; } = "en-US-JennyNeural";

        public string Rate { get; set; } = "+18%";
        public string Pitch { get; set; } = "+11Hz";

        public void Speak(string text)
        {
            ProcessStartInfo startInfo;
            string outputFile = $"output{DateTime.Now:yyyyMMddHHmmss}.mp3";

            //if (configuration["RUNNING_IN_DOCKER"] == "true")
            //{
            //    var appDir = AppContext.BaseDirectory;
            //    var toolPath = Path.Combine(appDir, "edge-tts");
            //    startInfo = new ProcessStartInfo
            //    {
            //        FileName = toolPath,
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        RedirectStandardError = true,
            //        CreateNoWindow = true
            //    };
            //}
            //else
            //{
            startInfo = new ProcessStartInfo
            {
                FileName = "edge-tts",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            //}

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

                CleanupOldAudioFiles();

                audioPlayer.PlayAudio(outputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in process: " + ex.Message);
            }
        }

        public TimeSpan GetAudioDuration(string filePath)
        {
            var file = TagLib.File.Create(filePath);
            return file.Properties.Duration;
        }

        public string GenerateSpeakAudioAndGetFilePath(string text)
        {
            string outputFile = $"output{DateTime.Now:yyyyMMddHHmmss}.mp3";
            ProcessStartInfo startInfo;

            //if (configuration["RUNNING_IN_DOCKER"] == "true")
            //{
            //    var appDir = AppContext.BaseDirectory;
            //    var toolPath = Path.Combine(appDir, "edge-tts");
            //    startInfo = new ProcessStartInfo
            //    {
            //        FileName = toolPath,
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        RedirectStandardError = true,
            //        CreateNoWindow = true
            //    };
            //}
            //else
            //{
            startInfo = new ProcessStartInfo
            {
                FileName = "edge-tts",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            //}

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

                CleanupOldAudioFiles();

                return outputFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in process: " + ex.Message);

                throw;
            }
        }

        public (string audioPath, string subtitlesPath) GenerateSpeakAudioAndGetFilePathWithSubtitles(string text)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string outputAudioFile = $"output{timestamp}.mp3";
            string outputSubtitleFile = $"output{timestamp}.vtt";

            ProcessStartInfo startInfo;

            //if (configuration["RUNNING_IN_DOCKER"] == "true")
            //{
            //    var appDir = AppContext.BaseDirectory;
            //    var toolPath = Path.Combine(appDir, "edge-tts");
            //    startInfo = new ProcessStartInfo
            //    {
            //        FileName = toolPath,
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        RedirectStandardError = true,
            //        CreateNoWindow = true
            //    };
            //}
            //else
            //{
            startInfo = new ProcessStartInfo
            {
                FileName = "edge-tts",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            //}

            startInfo.ArgumentList.Add("--text");
            startInfo.ArgumentList.Add(text);
            startInfo.ArgumentList.Add("--voice");
            startInfo.ArgumentList.Add(Voice);
            startInfo.ArgumentList.Add("--rate");
            startInfo.ArgumentList.Add(Rate);
            startInfo.ArgumentList.Add("--pitch");
            startInfo.ArgumentList.Add(Pitch);

            startInfo.ArgumentList.Add("--write-media");
            startInfo.ArgumentList.Add(outputAudioFile);

            startInfo.ArgumentList.Add("--write-subtitles");
            startInfo.ArgumentList.Add(outputSubtitleFile);

            try
            {
                using Process process = new Process { StartInfo = startInfo };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error: {error}");
                    throw new InvalidProgramException();
                }

                CleanupOldAudioFiles();

                return (outputAudioFile, outputSubtitleFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in process: " + ex.Message);
                throw;
            }
        }

        public static void CleanupOldAudioFiles(TimeSpan maxAge)
        {
            try
            {
                var directory = Directory.GetCurrentDirectory();
                var filesToDelete = Directory.GetFiles(directory, "output*.mp3")
                    .Where(file =>
                    {
                        var fileInfo = new FileInfo(file);
                        return DateTime.Now - fileInfo.CreationTime > maxAge;
                    })
                    .ToList();

                foreach (var file in filesToDelete)
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted old audio file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        public static void CleanupOldAudioFiles() => CleanupOldAudioFiles(TimeSpan.FromMinutes(10));
    }
}