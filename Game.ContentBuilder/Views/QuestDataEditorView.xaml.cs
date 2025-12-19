using System.Windows;
using System.Windows.Controls;
using Game.ContentBuilder.ViewModels;

namespace Game.ContentBuilder.Views;

public partial class QuestDataEditorView : UserControl
{
    public QuestDataEditorView()
    {
        InitializeComponent();
    }

    private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not QuestDataEditorViewModel viewModel) return;

        switch (e.NewValue)
        {
            case QuestDataCategoryGroup groupNode:
                viewModel.SelectedCategoryGroup = groupNode;
                break;
            
            case QuestDataCategory categoryNode:
                viewModel.SelectedCategory = categoryNode;
                break;
        }
    }
}
