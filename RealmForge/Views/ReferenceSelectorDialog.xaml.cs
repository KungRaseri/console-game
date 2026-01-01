using RealmForge.ViewModels;
using System.Windows;

namespace RealmForge.Views;

public partial class ReferenceSelectorDialog : Window
{
    public string? SelectedReference { get; private set; }

    public ReferenceSelectorDialog(string? initialDomain = null)
    {
        InitializeComponent();
        DataContext = new ReferenceSelectorViewModel(initialDomain);
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ReferenceSelectorViewModel;
        if (vm?.SelectedReference != null)
        {
            SelectedReference = vm.SelectedReference;
            DialogResult = true;
            Close();
        }
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ReferenceSelectorViewModel;
        if (vm != null && vm.SelectedDomain != null && vm.SelectedPath != null && vm.SelectedCategory != null)
        {
            // Generate wildcard reference: @domain/path/category:*
            SelectedReference = $"@{vm.SelectedDomain}/{vm.SelectedPath}/{vm.SelectedCategory}:*";
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
