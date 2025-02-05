using System.Diagnostics;
using HepaticaAI.Core.Interfaces.Voice;
using NAudio.Wave;
namespace HepaticaAI.Voice.Services
{
    internal class NAudioPlayer : IAudioPlayer
    {
        private ManualResetEvent _playbackCompleted = new ManualResetEvent(false);

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Console.WriteLine("Событие PlaybackStopped сработало.");
            _playbackCompleted.Set(); // Разблокируем поток
        }
        public void PlayAudio(string filePath)
        {
            using var audioFile = new AudioFileReader(filePath);

            audioFile.Volume = 0.3f;
            
            using var outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;

            outputDevice.Init(audioFile);
            outputDevice.Play();

            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(500);
            }

            
            _playbackCompleted.WaitOne();

            DeleteMp3FilesInFolder(filePath);//Todo uncomment this 
        }
        private void DeleteMp3FilesInFolder(string filePath)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string[] mp3Files = Directory.GetFiles(currentDirectory, "*.mp3");

                foreach (string mp3File in mp3Files)
                {
                    try
                    {
                        File.Delete(mp3File);
                        Debug.WriteLine($"File dilited: {mp3File}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при удалении файла {mp3File}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in deleteion {filePath}: {ex.Message}");
            }
        }
    }
}
