using Game.ContentBuilder.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Game.ContentBuilder.Views;

public partial class ReferenceSelectorDialog : Window
{
    public string? SelectedReference { get; private set; }

    public ReferenceSelectorDialog(string? initialReferenceType = null)
    {
        InitializeComponent();
        DataContext = new ReferenceSelectorViewModel(initialReferenceType);
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var vm = DataContext as ReferenceSelectorViewModel;
        if (vm == null) return;

        if (e.NewValue is ReferenceCategory category)
        {
            vm.SelectedCategory = category;
            vm.SelectedItem = null;
        }
        else if (e.NewValue is ReferenceItem item)
        {
            vm.SelectedItem = item;
            // Find parent category
            var treeView = sender as TreeView;
            if (treeView?.ItemsSource is System.Collections.IEnumerable categories)
            {
                foreach (ReferenceCategory cat in categories)
                {
                    if (cat.Items.Contains(item))
                    {
                        vm.SelectedCategory = cat;
                        break;
                    }
                }
            }
        }
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
        if (vm != null)
        {
            // Generate "all items" reference: @materialRef or @itemRef
            SelectedReference = $"{vm.SelectedReferenceType}Ref";
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
