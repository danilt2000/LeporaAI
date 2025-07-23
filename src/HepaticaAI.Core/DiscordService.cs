using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using IAudioClient = Discord.Audio.IAudioClient;
using TokenType = Discord.TokenType;

namespace HepaticaAI.Core;

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

        _discord.Log += LogAsync;
        _discord.Ready += ReadyAsync;

        _discord.LoginAsync(TokenType.Bot, configuration["DiscordBotToken"]).GetAwaiter().GetResult();
        _discord.StartAsync().GetAwaiter().GetResult();
    }

    public void Initialization()
    {

    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine("Bot is ready!");
        return Task.CompletedTask;
    }

    public async Task<string?> GetUsernameByIdAsync(ulong userId)
    {
        var user = _discord.GetUser(userId);
        if (user != null)
        {
            return user.GlobalName;
        }
        try
        {
            var fetchedUser = await _discord.Rest.GetUserAsync(userId);

            return fetchedUser?.GlobalName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching user: {ex.Message}");
            return null;
        }
    }

    public async Task SendMessageToGuildTextChannelAsync(string message)
    {
        foreach (var guild in _discord.Guilds)
        {
            var voiceChannel = guild.CurrentUser?.VoiceChannel;
            if (voiceChannel == null)
                continue;

            foreach (var textChannel in guild.TextChannels.OrderBy(tc => tc.Position))
            {
                try
                {
                    await textChannel.SendMessageAsync($"🎙 {message}");
                    break; 
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}