using Discord;
using Discord.Audio.Streams;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using IAudioClient = Discord.Audio.IAudioClient;
using TokenType = Discord.TokenType;

public class DiscordService
{
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _discord;
    private IAudioClient _audioClient;
    private bool _initialized = false;

    public DiscordService(IConfiguration configuration)
    {
        _configuration = configuration;
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildVoiceStates
        };

        _discord = new DiscordSocketClient(config);

        //_discord.LoginAsync(TokenType.Bot, configuration["DiscordBotToken"]).GetAwaiter().GetResult();

        //_discord.StartAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Connects to a specific guild and voice channel.
    /// </summary>
    public async Task ConnectToVoiceChannelAsync(ulong guildId, ulong voiceChannelId)
    {
        await _discord.LoginAsync(TokenType.Bot, _configuration["DiscordBotToken"]);

        await _discord.StartAsync();

        foreach (var guildEntity in _discord.Guilds)
        {
            Debug.WriteLine(guildEntity);
        }

        var guild = _discord.GetGuild(guildId); // Get the guild
        if (guild == null)
        {
            Console.WriteLine($"Guild with ID {guildId} not found.");
            return;
        }

        var voiceChannel = guild.GetVoiceChannel(voiceChannelId); // Get the voice channel
        if (voiceChannel == null)
        {
            Console.WriteLine($"Voice channel with ID {voiceChannelId} not found.");
            return;
        }

        Console.WriteLine($"Connecting to voice channel: {voiceChannel.Name}");
        await JoinChannelAsync(voiceChannel);
    }

    /// <summary>
    /// Подключаемся к указанному голосовому каналу.
    /// </summary>
    public async Task JoinChannelAsync(IVoiceChannel voiceChannel)
    {
        if (voiceChannel == null)
            throw new ArgumentNullException(nameof(voiceChannel));

        // Подключаемся к голосовому каналу
        _audioClient = await voiceChannel.ConnectAsync();

        // Активация приема аудио (Discord.Net отключает его по умолчанию)
        // Если хотите включить/выключить флаг "говорения" для бота:
        await _audioClient.SetSpeakingAsync(false);
        // Если нужно, можно включить "говорение"

        // Отмечаем, что сервис инициализирован
        _initialized = true;
        Console.WriteLine("Bot connected to voice channel.");
    }

    /// <summary>
    /// Запустить прослушку всех (не ботов) пользователей в данном канале.
    /// </summary>
    public async Task StartListeningAllAsync(IVoiceChannel voiceChannel)
    {
        if (!_initialized)
            throw new InvalidOperationException("Voice service wasn't initialized (JoinChannelAsync not called).");

        var users = await voiceChannel.GetUsersAsync().FlattenAsync();
        foreach (var user in users)
        {
            if (user.IsBot)
                continue;

            // Запускаем прослушку без await, чтобы все работали параллельно
            _ = ListenUserAsync(user);
        }
    }

    /// <summary>
    /// Слушаем одного пользователя, передаём поток в ffmpeg и сохраняем на диск.
    /// </summary>
    public async Task ListenUserAsync(IGuildUser user)
    {
        if (!_initialized)
            throw new InvalidOperationException("Voice service wasn't initialized.");

        // Убеждаемся, что мы имеем доступ к SocketGuildUser
        var socketUser = user as SocketGuildUser;
        if (socketUser == null)
        {
            Console.WriteLine($"User {user.Username} is not a SocketGuildUser?");
            return;
        }

        // Получаем входной аудиопоток пользователя (Discord.Net)
        var userAudioStream = (InputStream)socketUser.AudioStream;
        if (userAudioStream == null)
        {
            Console.WriteLine($"No audio stream for {user.Username}. Possibly not speaking or not in voice?");
            return;
        }

        // Запускаем ffmpeg-процесс, который будет писать данные в файл (или куда нужно)
        using var ffmpeg = CreateFfmpegOut(user.Username);
        using var ffmpegOutStdinStream = ffmpeg.StandardInput.BaseStream;

        var buffer = new byte[3840];
        try
        {
            // Читаем аудио из Discord (raw pcm s16le 48kHz 2ch)
            while (await userAudioStream.ReadAsync(buffer, 0, buffer.Length) > 0)
            {
                // Передаём в stdin ffmpeg
                await ffmpegOutStdinStream.WriteAsync(buffer, 0, buffer.Length);
                await ffmpegOutStdinStream.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ListenUserAsync error for {user.Username}: {ex.Message}");
        }
        finally
        {
            await ffmpegOutStdinStream.FlushAsync();
            ffmpegOutStdinStream.Close();

            ffmpeg.WaitForExit();
            ffmpeg.Close();

            Console.WriteLine($"{user.Username} ffmpeg stream closed.");
        }
    }

    /// <summary>
    /// Запускаем ffmpeg, который ожидает PCM s16le на stdin и пишет WAV/MP3 и т.д.
    /// Простой вариант - пишем уникальный файл на основе имени пользователя.
    /// </summary>
    private Process CreateFfmpegOut(string userName)
    {
        // Файл будет называться, например: "record_<userName>_<timestamp>.wav"
        // Путь к файлу - в одной папке с программой. В реальном проекте желательно директорию менять.
        var fileName = $"record_{userName}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.wav";

        // Аргументы ffmpeg:
        // -f s16le      : формат входного потока (raw pcm signed 16bit little-endian)
        // -ar 48000     : частота дискретизации 48 kHz
        // -ac 2         : 2 канала
        // -i pipe:0     : берем данные со стандартного ввода
        // -ac 1         : на выходе делаем 1 канал (моно, например)
        // -ar 16000     : и частоту 16kHz (если нужно)
        // -f wav        : формат выхода WAV (или можно mp3/ogg и т.д.)
        // Затем указываем файл
        //
        // При желании можно оставлять исходные 48kHz стерео, если нужно.
        var arguments = $"-f s16le -ar 48000 -ac 2 -i pipe:0 " +
                        $"-ac 1 -ar 16000 -f wav \"{fileName}\"";

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        return Process.Start(psi);
    }
}
