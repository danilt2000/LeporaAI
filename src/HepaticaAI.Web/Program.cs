using HepaticaAI.Brain;
using HepaticaAI.Core;
using HepaticaAI.Movement;
using HepaticaAI.Vision;
using HepaticaAI.Voice;
using HepaticaAI.Web.Services;

namespace HepaticaAI.Web
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; } = null!;

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            Configuration = configurationBuilder.Build();

            ConfigureServices(builder.Services);

            builder.Services.AddRazorPages();

            builder.Services.AddSingleton<WebSocketConnectionManager>();

            var app = builder.Build();

            app.Lifetime.ApplicationStarted.Register(async () =>
            {
                using var scope = app.Services.CreateScope();
                var facade = scope.ServiceProvider.GetRequiredService<AILifecycleFacade>();
                await facade.StartLife();
            });

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseWebSockets();
            app.UseMiddleware<WebSocketHandler>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IConfiguration>(Configuration);

            serviceCollection.AddCore(Configuration);
            serviceCollection.AddBrain(Configuration);
            serviceCollection.AddVision(Configuration);
            serviceCollection.AddMovement(Configuration);
            serviceCollection.AddVoice(Configuration);
        }
    }
}
