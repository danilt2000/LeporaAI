using NAudio.Wave;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

namespace HepaticaAI.Brain.Services
{
    public class WhisperSpeechRecognition : IDisposable
    {
        private WhisperFactory? _factory;
        private WhisperProcessor? _processor;
        private bool _initialized;

        public async Task InitializeModelAsync(string modelPath = "ggml-base.bin")
        {
            var modelFileName = "ggml-base.bin";
            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, GgmlType.Base);
            }

            _factory = WhisperFactory.FromPath(modelFileName);
            _processor = _factory.CreateBuilder()
                 .WithLanguage("en")
                 .Build();

            _initialized = true;
        }

        private static async Task DownloadModel(string fileName, GgmlType ggmlType)
        {
            Console.WriteLine($"Downloading Model {fileName}");
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            using var fileWriter = File.OpenWrite(fileName);
            await modelStream.CopyToAsync(fileWriter);
        }

        internal async Task<string> RecognizeSpeechFromWavVideo()
        {
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.bin";
            var wavFileName = "test.wav";

            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, ggmlType);
            }

            using var whisperLogger = LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);

            using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            using var reader = new AudioFileReader(wavFileName);
            using var resampler = new MediaFoundationResampler(reader, new WaveFormat(16000, reader.WaveFormat.Channels))
            {
                ResamplerQuality = 60  // Adjust quality if desired
            };

            using var ms = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(ms, resampler);

            ms.Position = 0;
            var sb = new StringBuilder();

            await foreach (var result in processor.ProcessAsync(ms))
            {
                sb.AppendLine($"{result.Start}->{result.End}: {result.Text}");
            }

            return sb.ToString();
        }

        public async Task<string> RecognizeSpeechFromPcmBytes(byte[] pcmData, GgmlType ggmlType = GgmlType.Base)
        {
            // 1) Make sure the model exists (download if not)
            var modelFileName = "ggml-base.bin";
            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, ggmlType);
            }

            // 2) Create the Whisper processor
            using var whisperLogger = LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);
            using var whisperFactory = WhisperFactory.FromPath(modelFileName);
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            // 3) Convert raw PCM data to a WAV stream in memory
            //    - We specify the wave format: 16 kHz, 16-bit, 1 channel
            using var wavStream = new MemoryStream();
            using (var writer = new WaveFileWriter(wavStream, new WaveFormat(16000, 16, 1)))
            {
                writer.Write(pcmData, 0, pcmData.Length);
            }
            wavStream.Position = 0;  // reset to start

            // 4) Process the WAV from memory
            var sb = new StringBuilder();
            await foreach (var result in processor.ProcessAsync(wavStream))
            {
                sb.AppendLine($"{result.Start}->{result.End}: {result.Text}");
            }

            return sb.ToString();
        }


        public async Task<string> RecognizeSpeechUsingWhisper(byte[] audioData, int sourceSampleRate)
        {
            if (!_initialized)
                throw new InvalidOperationException("Call InitializeModelAsync first");

            // 1. Resample audio
            var resampled = ResampleTo16kHz(audioData, sourceSampleRate);

            // 2. Create proper WAV stream
            using var wavStream = CreateWavStream(resampled, 16000);

            // 3. Process with Whisper
            var result = new StringBuilder();
            await foreach (var segment in _processor!.ProcessAsync(wavStream))
            {
                result.Append(segment.Text);
            }

            return result.ToString().Trim();
        }

        private byte[] ResampleTo16kHz(byte[] inputAudio, int sourceRate)
        {
            using var inputStream = new MemoryStream(inputAudio);
            var sourceFormat = new WaveFormat(sourceRate, 16, 1);
            using var reader = new RawSourceWaveStream(inputStream, sourceFormat);

            // Ask for 16-bit, mono, 16 kHz
            using var resampler = new MediaFoundationResampler(
                reader,
                new WaveFormat(16000, 16, 1)
            );

            // Write wave data directly from the resampler
            using var convertedStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(convertedStream, resampler);

            // Skip the WAV header (44 bytes) if you only want raw 16-bit PCM
            convertedStream.Position = 44;
            var result = new byte[convertedStream.Length - 44];
            convertedStream.Read(result, 0, result.Length);
            return result;
        }

        private MemoryStream CreateWavStream(byte[] pcmData, int sampleRate)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(pcmData.Length + 36);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(pcmData.Length);
                writer.Write(pcmData);
            }
            stream.Position = 0;
            return stream;
        }

        public void Dispose()
        {
            _processor?.Dispose();
            _factory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}