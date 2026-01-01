using System.Windows;
using Game.ContentBuilder.ViewModels;

namespace Game.ContentBuilder.Views;

/// <summary>
/// Interaction logic for PreviewWindow.xaml
/// </summary>
public partial class PreviewWindow : Window
{
    public PreviewWindow()
    {
        InitializeComponent();
        DataContext = new PreviewWindowViewModel();
    }
}
