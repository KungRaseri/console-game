using System.Windows;
using System.Windows.Controls;
using Game.ContentBuilder.ViewModels;

namespace Game.ContentBuilder.Views;

/// <summary>
/// Interaction logic for CatalogEditorView.xaml
/// </summary>
public partial class CatalogEditorView : UserControl
{
    public CatalogEditorView()
    {
        InitializeComponent();
    }

    private void CatalogTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not CatalogEditorViewModel viewModel) return;

        // Set the appropriate selected property based on the type of the selected item
        switch (e.NewValue)
        {
            case TypeCatalog catalog:
                viewModel.SelectedCatalog = catalog;
                viewModel.SelectedCategory = null;
                viewModel.SelectedItem = null;
                break;
            case TypeCategory category:
                viewModel.SelectedCategory = category;
                viewModel.SelectedItem = null;
                break;
            case TypeItem item:
                viewModel.SelectedItem = item;
                break;
        }
    }
}

