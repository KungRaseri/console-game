using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RealmForge.Models;
using RealmForge.ViewModels;

namespace RealmForge.Views;

/// <summary>
/// Interaction logic for NameListEditorView.xaml
/// </summary>
public partial class NameListEditorView : UserControl
{
    private PatternToken? _draggedToken;
    private Point _startPoint;

    public NameListEditorView()
    {
        InitializeComponent();
    }

    private void Badge_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Don't start drag if clicking on the delete button or its icon
        if (e.OriginalSource is FrameworkElement element)
        {
            System.Diagnostics.Debug.WriteLine($"Badge_PreviewMouseLeftButtonDown - OriginalSource: {element.GetType().Name}");

            // Walk up the visual tree to see if we're inside a Button
            var current = element;
            while (current != null && current != sender)
            {
                System.Diagnostics.Debug.WriteLine($"  Checking: {current.GetType().Name}");
                if (current is Button btn)
                {
                    System.Diagnostics.Debug.WriteLine($"  BUTTON DETECTED - Preventing drag. Command: {btn.Command?.GetType().Name}");
                    // Prevent drag operation from starting
                    _draggedToken = null;
                    return;
                }
                current = System.Windows.Media.VisualTreeHelper.GetParent(current) as FrameworkElement;
            }
            System.Diagnostics.Debug.WriteLine("  No button found in tree, allowing drag");
        }

        _startPoint = e.GetPosition(null);
        if (sender is FrameworkElement senderElement && senderElement.Tag is PatternToken token)
        {
            _draggedToken = token;
            System.Diagnostics.Debug.WriteLine($"  Drag token set: {token.DisplayText}");
        }
    }

    private void Badge_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _draggedToken != null)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            // Start drag if moved enough
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is FrameworkElement element)
                {
                    DragDrop.DoDragDrop(element, _draggedToken, DragDropEffects.Move);
                }
            }
        }
    }

    private void Badge_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PatternToken)))
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Badge_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PatternToken)) && sender is FrameworkElement element)
        {
            var droppedToken = e.Data.GetData(typeof(PatternToken)) as PatternToken;
            var targetToken = element.Tag as PatternToken;

            if (droppedToken != null && targetToken != null && droppedToken != targetToken)
            {
                // Get the pattern from the visual tree
                var pattern = FindPatternFromElement(element);
                if (pattern != null && DataContext is NameListEditorViewModel viewModel)
                {
                    int droppedIndex = pattern.Tokens.IndexOf(droppedToken);
                    int targetIndex = pattern.Tokens.IndexOf(targetToken);

                    if (droppedIndex >= 0 && targetIndex >= 0)
                    {
                        pattern.Tokens.Move(droppedIndex, targetIndex);
                        viewModel.UpdatePatternTemplateFromTokens(pattern);
                        viewModel.GenerateExampleForPattern(pattern);
                        viewModel.StatusMessage = $"Moved {droppedToken.DisplayText} token";
                    }
                }
            }
        }
        _draggedToken = null;
        e.Handled = true;
    }

    private NamePatternBase? FindPatternFromElement(FrameworkElement element)
    {
        // Walk up the visual tree to find the Card that contains the pattern
        DependencyObject current = element;
        while (current != null)
        {
            if (current is FrameworkElement fe && fe.DataContext is NamePatternBase pattern)
            {
                return pattern;
            }
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void DeleteToken_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            System.Diagnostics.Debug.WriteLine("DeleteToken_Click - sender is not Button");
            return;
        }

        var token = button.Tag as PatternToken;
        if (token == null)
        {
            System.Diagnostics.Debug.WriteLine("DeleteToken_Click - token is null");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"DeleteToken_Click - Token: {token.DisplayText}");

        // Walk up visual tree to find the Card which has the pattern as DataContext
        NamePatternBase? pattern = null;
        DependencyObject current = button;
        while (current != null)
        {
            if (current is FrameworkElement fe && fe.DataContext is NamePatternBase p)
            {
                pattern = p;
                System.Diagnostics.Debug.WriteLine($"DeleteToken_Click - Found pattern: {pattern.PatternTemplate}");
                break;
            }
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        if (pattern == null)
        {
            System.Diagnostics.Debug.WriteLine("DeleteToken_Click - pattern not found in visual tree");
            return;
        }

        // Call the ViewModel method
        if (DataContext is NameListEditorViewModel viewModel)
        {
            System.Diagnostics.Debug.WriteLine("DeleteToken_Click - Calling RemoveTokenDirect");
            viewModel.RemoveTokenDirect(pattern, token);
        }
    }
}
