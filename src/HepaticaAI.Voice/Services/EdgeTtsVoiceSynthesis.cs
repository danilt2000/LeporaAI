using HepaticaAI.Core.Interfaces.Voice;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HepaticaAI.Voice.Services
{
    internal class EdgeTtsVoiceSynthesis(IAudioPlayer audioPlayer) : IVoiceSynthesis
    {
        public string Voice { get; set; } = "ru-RU-SvetlanaNeural";
        public string Rate { get; set; } = "+18%";
        public string Pitch { get; set; } = "+11Hz";

        public void Speak(string text)
        {
            //Task.Run(() =>
            //{
            // Формируем уникальное имя выходного файла
            string outputFile = $"output{DateTime.Now:yyyyMMddHHmmss}.mp3";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "edge-tts", // Если программа не найдена, укажите полный путь
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Добавляем аргументы по отдельности
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

                // Читаем вывод, чтобы избежать зависания
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Debug.WriteLine($"Error: {error}");
                    return;
                }

                // Проигрываем созданный аудиофайл
                audioPlayer.PlayAudio(outputFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in process: " + ex.Message);
            }
            //});
        }
    }
}
