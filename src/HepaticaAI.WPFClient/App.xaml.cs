using System.IO;
using System.Text;
using System.Windows;
using HepaticaAI.Brain;
using HepaticaAI.Core;
using HepaticaAI.Movement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.WPFClient
{
        /// <summary>
        /// Interaction logic for App.xaml
        /// </summary>
        public partial class App : Application
        {
                public IServiceProvider ServiceProvider { get; private set; } = null!;
                public IConfiguration Configuration { get; private set; } = null!;

                protected override void OnStartup(StartupEventArgs eventArgs)
                {
                        var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", false, true);

                        Configuration = builder.Build();

                        var serviceCollection = new ServiceCollection();
                        ConfigureServices(serviceCollection);
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        ServiceProvider = serviceCollection.BuildServiceProvider();
                        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                        //mainWindow.Show();//Todo if not needed delete 
                        base.OnStartup(eventArgs);
                }

                private void ConfigureServices(ServiceCollection serviceCollection)
                {
                        serviceCollection.AddSingleton<MainWindow>();

                        serviceCollection.AddCore(Configuration);
                        serviceCollection.AddBrain(Configuration);

                        serviceCollection.AddMovement(Configuration);
                        //serviceCollection.AddCore(Configuration);//Todo add another binding later 
                        //serviceCollection.AddCore(Configuration);
                }
        }
}
