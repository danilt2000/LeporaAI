using System.Diagnostics;

namespace HepaticaAI.Brain.Services
{
    public class BrowserService
    {
        public void OpenStreamSettingsPage(string livestreamId)
        {
            string url = $"https://studio.youtube.com/video/{livestreamId}/livestreaming";

            var psi = new ProcessStartInfo
            {
                FileName = "firefox",
                Arguments = $"--new-window \"{url}\"",
                UseShellExecute = false
            };

            var process = Process.Start(psi);

            //var psi = new ProcessStartInfo
            //{
            //    FileName = "bash",
            //    Arguments = $"-c \"timeout 120s firefox --new-window '{url}'\"",
            //    UseShellExecute = false
            //};

            //Process.Start(psi);

            //if (process == null)
            //{
            //    Console.WriteLine("Не удалось запустить Firefox.");
            //    return;
            //}

            //Console.WriteLine($"Firefox запущен с PID {process.Id}");

            //Task.Run(async () =>
            //{
            //    await Task.Delay(TimeSpan.FromMinutes(2));

            //    try
            //    {
            //        process.Kill();
            //        Console.WriteLine("Firefox был закрыт автоматически через 2 минуты.");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Ошибка при завершении Firefox: {ex.Message}");
            //    }
            //});
        }
    }
}
