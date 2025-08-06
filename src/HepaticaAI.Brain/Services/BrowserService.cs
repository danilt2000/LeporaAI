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

            Process.Start(psi);

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(2));

                try
                {
                    var psiKill = new ProcessStartInfo
                    {
                        FileName = "pkill",
                        Arguments = "firefox",
                        UseShellExecute = false
                    };

                    Process.Start(psiKill);
                    Console.WriteLine("Firefox был закрыт автоматически через 2 минуты.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при завершении Firefox: {ex.Message}");
                }
            });
        }
    }
}
