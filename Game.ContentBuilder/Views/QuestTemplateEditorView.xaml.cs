using System.Windows;
using System.Windows.Controls;
using Game.ContentBuilder.ViewModels;

namespace Game.ContentBuilder.Views
{
    /// <summary>
    /// Interaction logic for QuestTemplateEditorView.xaml
    /// </summary>
    public partial class QuestTemplateEditorView : UserControl
    {
        public QuestTemplateEditorView()
        {
            InitializeComponent();
        }

        private void DifficultyNode_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem { DataContext: QuestDifficultyNode difficultyNode })
            {
                if (DataContext is QuestTemplateEditorViewModel viewModel)
                {
                    viewModel.SelectedQuestType = difficultyNode.ParentQuestType;
                    viewModel.SelectedDifficulty = difficultyNode;
                }
            }
        }
    }
}
