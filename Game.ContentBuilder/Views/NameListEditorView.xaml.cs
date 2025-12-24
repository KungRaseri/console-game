using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.ViewModels;

namespace Game.ContentBuilder.Views;

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
        _startPoint = e.GetPosition(null);
        if (sender is FrameworkElement element && element.Tag is PatternToken token)
        {
            _draggedToken = token;
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
}
