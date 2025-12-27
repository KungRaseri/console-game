using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Game.ContentBuilder.Views.Components
{
    /// <summary>
    /// Interaction logic for PatternItemControl.xaml
    /// Displays a pattern with token composer, weight, description, and generated examples.
    /// </summary>
    public partial class PatternItemControl : UserControl
    {
        public static readonly DependencyProperty TokensProperty =
            DependencyProperty.Register(nameof(Tokens), typeof(object), typeof(PatternItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty WeightProperty =
            DependencyProperty.Register(nameof(Weight), typeof(int), typeof(PatternItemControl),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty WeightPercentageProperty =
            DependencyProperty.Register(nameof(WeightPercentage), typeof(double), typeof(PatternItemControl),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(PatternItemControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty GeneratedExamplesProperty =
            DependencyProperty.Register(nameof(GeneratedExamples), typeof(string), typeof(PatternItemControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(PatternItemControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ComponentNamesProperty =
            DependencyProperty.Register(nameof(ComponentNames), typeof(object), typeof(PatternItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty InsertComponentTokenCommandProperty =
            DependencyProperty.Register(nameof(InsertComponentTokenCommand), typeof(ICommand), typeof(PatternItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty InsertReferenceTokenCommandProperty =
            DependencyProperty.Register(nameof(InsertReferenceTokenCommand), typeof(ICommand), typeof(PatternItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty BrowseReferenceCommandProperty =
            DependencyProperty.Register(nameof(BrowseReferenceCommand), typeof(ICommand), typeof(PatternItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RegenerateExamplesCommandProperty =
            DependencyProperty.Register(nameof(RegenerateExamplesCommand), typeof(ICommand), typeof(PatternItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DuplicatePatternCommandProperty =
            DependencyProperty.Register(nameof(DuplicatePatternCommand), typeof(ICommand), typeof(PatternItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteTokenCommandProperty =
            DependencyProperty.Register(nameof(DeleteTokenCommand), typeof(ICommand), typeof(PatternItemControl),
                new PropertyMetadata(null));

        // Events for drag-and-drop token reordering
        public event EventHandler<TokenDragEventArgs>? TokenDragStarted;
        public event EventHandler<TokenDragEventArgs>? TokenDropped;

        public PatternItemControl()
        {
            InitializeComponent();
        }

        public object Tokens
        {
            get => GetValue(TokensProperty);
            set => SetValue(TokensProperty, value);
        }

        public int Weight
        {
            get => (int)GetValue(WeightProperty);
            set => SetValue(WeightProperty, value);
        }

        public double WeightPercentage
        {
            get => (double)GetValue(WeightPercentageProperty);
            set => SetValue(WeightPercentageProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string GeneratedExamples
        {
            get => (string)GetValue(GeneratedExamplesProperty);
            set => SetValue(GeneratedExamplesProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public object ComponentNames
        {
            get => GetValue(ComponentNamesProperty);
            set => SetValue(ComponentNamesProperty, value);
        }

        public ICommand InsertComponentTokenCommand
        {
            get => (ICommand)GetValue(InsertComponentTokenCommandProperty);
            set => SetValue(InsertComponentTokenCommandProperty, value);
        }

        public ICommand InsertReferenceTokenCommand
        {
            get => (ICommand)GetValue(InsertReferenceTokenCommandProperty);
            set => SetValue(InsertReferenceTokenCommandProperty, value);
        }

        public ICommand BrowseReferenceCommand
        {
            get => (ICommand)GetValue(BrowseReferenceCommandProperty);
            set => SetValue(BrowseReferenceCommandProperty, value);
        }

        public ICommand RegenerateExamplesCommand
        {
            get => (ICommand)GetValue(RegenerateExamplesCommandProperty);
            set => SetValue(RegenerateExamplesCommandProperty, value);
        }

        public ICommand DuplicatePatternCommand
        {
            get => (ICommand)GetValue(DuplicatePatternCommandProperty);
            set => SetValue(DuplicatePatternCommandProperty, value);
        }

        public ICommand DeleteTokenCommand
        {
            get => (ICommand)GetValue(DeleteTokenCommandProperty);
            set => SetValue(DeleteTokenCommandProperty, value);
        }
    }

    /// <summary>
    /// Event args for token drag-and-drop operations
    /// </summary>
    public class TokenDragEventArgs : EventArgs
    {
        public object? Token { get; set; }
        public int SourceIndex { get; set; }
        public int TargetIndex { get; set; }
    }
}
