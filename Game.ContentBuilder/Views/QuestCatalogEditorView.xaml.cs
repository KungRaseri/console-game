using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Game.ContentBuilder.ViewModels;

namespace Game.ContentBuilder.Views;

public partial class QuestCatalogEditorView : UserControl
{
    public QuestCatalogEditorView()
    {
        InitializeComponent();
    }

    private void QuestType_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not QuestCatalogEditorViewModel viewModel) return;

        // Handle TreeView selection - could be QuestTypeNode or QuestDifficultyNode
        switch (e.NewValue)
        {
            case QuestTypeNode typeNode:
                viewModel.SelectedQuestType = typeNode;
                if (typeNode.Difficulties.Any())
                {
                    viewModel.SelectedDifficulty = typeNode.Difficulties[0];
                }
                break;
            
            case QuestDifficultyNode diffNode:
                viewModel.SelectedQuestType = diffNode.ParentQuestType;
                viewModel.SelectedDifficulty = diffNode;
                break;
        }
    }
}
