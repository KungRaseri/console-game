using System.Windows;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Models;

namespace Game.ContentBuilder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel && e.NewValue is CategoryNode selectedNode)
        {
            viewModel.SelectedCategory = selectedNode;
        }
    }
}