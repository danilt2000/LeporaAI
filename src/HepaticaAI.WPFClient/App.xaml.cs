using HepaticaAI.Brain;
using HepaticaAI.Core;
using HepaticaAI.Movement;
using HepaticaAI.Vision;
using HepaticaAI.Voice;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using System.Windows;
using HepaticaAI.WPFClient.Views;

namespace HepaticaAI.WPFClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; } = null!;
        public IConfiguration Configuration { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs eventArgs)
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            var aiLifecycleFacade = ServiceProvider.GetRequiredService<AILifecycleFacade>();
            await aiLifecycleFacade.StartLife();
            _ = ServiceProvider.GetRequiredService<MessageProcessorSelector>();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();


            base.OnStartup(eventArgs);
        }

        private void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IConfiguration>(Configuration);
            serviceCollection.AddSingleton<MainWindow>();

            serviceCollection.AddCore(Configuration);
            serviceCollection.AddBrain(Configuration);
            serviceCollection.AddVision(Configuration);
            serviceCollection.AddMovement(Configuration);
            serviceCollection.AddVoice(Configuration);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            var aiLifecycleFacade = ServiceProvider.GetRequiredService<AILifecycleFacade>();
            aiLifecycleFacade.EndLife();

            base.OnExit(e);
        }
    }
}
