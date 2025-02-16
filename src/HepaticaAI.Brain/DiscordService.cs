using Discord;
using Discord.WebSocket;
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

        _discord.Log += LogAsync;
        _discord.Ready += ReadyAsync;

        _discord.LoginAsync(TokenType.Bot, configuration["DiscordBotToken"]).GetAwaiter().GetResult();
        _discord.StartAsync().GetAwaiter().GetResult();
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
            return user.Username;
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

   
}
