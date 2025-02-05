using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whisper.net;

namespace HepaticaAI.Brain.Services
{
    public class WhisperSpeechRecognition : IDisposable
    {
        private WhisperFactory? _factory;
        private WhisperProcessor? _processor;
        private bool _initialized;

        public async Task InitializeModelAsync(string modelPath = "ggml-base.bin")
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Whisper model not found", modelPath);

            _factory = WhisperFactory.FromPath(modelPath);
            _processor = _factory.CreateBuilder()
                .WithLanguage("auto")  // Better for multi-language
                .WithThreads(Environment.ProcessorCount)
                .Build();

            _initialized = true;
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