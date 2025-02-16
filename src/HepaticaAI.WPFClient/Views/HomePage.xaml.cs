using HepaticaAI.Core.Interfaces.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace HepaticaAI.WPFClient.Views
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly Frame _frame;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemory _memory;

        public HomePage(Frame frame, IServiceProvider serviceProvider)
        {
            _frame = frame;
            _serviceProvider = serviceProvider;

            _memory = serviceProvider.GetRequiredService<IMemory>();

            InitializeComponent();
        }

        private void SwitchCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void SwitchCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void ChangeSystemPrompt(object sender, RoutedEventArgs e)
        {
            _frame.Navigate(new ChangeSystemPromptPage(_frame, _serviceProvider));
        }

        private void DeleteAllHistory(object sender, RoutedEventArgs e)
        {
            _memory.Clear();
        }
    }
}
