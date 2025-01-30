using System.Windows;
using HepaticaAI.Core.Interfaces.AI;

namespace HepaticaAI.WPFClient
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
        }
}