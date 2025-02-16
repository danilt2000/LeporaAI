using HepaticaAI.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using HepaticaAI.Core.Interfaces.Memory;

namespace HepaticaAI.WPFClient.Views
{
    /// <summary>
    /// Interaction logic for ChangeSystemPromptPage.xaml
    /// </summary>
    public partial class ChangeSystemPromptPage : Page
    {
        private readonly Frame _frame;
        private readonly ISystemPromptsUpdater systemPromptsUpdater;

        public ChangeSystemPromptPage(Frame frame, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _frame = frame;

            systemPromptsUpdater = serviceProvider.GetRequiredService<ISystemPromptsUpdater>();

            var prompts = systemPromptsUpdater.GetAllSystemPrompts();

            CurrentPromptEditor.Text = prompts[0];

            CurrentPromptEditorFirst.Text = prompts[1];

            CurrentPromptEditorSecond.Text = prompts[2];

            CharacterName.Text = systemPromptsUpdater.GetCharacterName();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            _frame.GoBack();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string prompt1 = CurrentPromptEditor.Text;
                string prompt2 = CurrentPromptEditorFirst.Text;
                string prompt3 = CurrentPromptEditorSecond.Text;

                systemPromptsUpdater.UpdateSystemPrompt("character_personality.json", prompt1);
                systemPromptsUpdater.UpdateSystemPrompt("character_personality_temp_first.json", prompt2);
                systemPromptsUpdater.UpdateSystemPrompt("character_personality_temp_second.json", prompt3);
                systemPromptsUpdater.SetCharacterName(CharacterName.Text);

                MessageBox.Show("✅ Промпты успешно обновлены!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при обновлении JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
