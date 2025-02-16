using System.Windows;
using HepaticaAI.Core.Interfaces.AI;
using Microsoft.Extensions.DependencyInjection;

namespace HepaticaAI.WPFClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ILLMClient llmClient, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            MainFrame.NavigationService.Navigate(new HomePage(MainFrame, serviceProvider));
        }
    }
}