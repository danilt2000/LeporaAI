using System.Windows;
using HepaticaAI.Core.Interfaces.AI;

namespace HepaticaAI.WPFClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ILLMClient llmClient)
        {
            InitializeComponent();
        }
        private void ChangeSystemPrompt(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SwitchCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SwitchCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        //private void SwitchButton_Checked(object sender, RoutedEventArgs e)
        //{
        //    (sender as ToggleButton).Content = "ON";
        //}

        //private void SwitchButton_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    (sender as ToggleButton).Content = "OFF";
        //}

        //private void SwitchCheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Switch turned ON!");
        //}

        //private void SwitchCheckBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Switch turned OFF!");
        //}

    }
}