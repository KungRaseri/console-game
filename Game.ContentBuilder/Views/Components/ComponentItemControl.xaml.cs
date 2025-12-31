using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Game.ContentBuilder.Views.Components
{
    /// <summary>
    /// Interaction logic for ComponentItemControl.xaml
    /// Displays a single name component with value, weight, rarity, and traits.
    /// </summary>
    public partial class ComponentItemControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(ComponentItemControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty RarityWeightProperty =
            DependencyProperty.Register(nameof(RarityWeight), typeof(int), typeof(ComponentItemControl),
                new FrameworkPropertyMetadata(50, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty TraitsProperty =
            DependencyProperty.Register(nameof(Traits), typeof(object), typeof(ComponentItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(ComponentItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty AddTraitCommandProperty =
            DependencyProperty.Register(nameof(AddTraitCommand), typeof(ICommand), typeof(ComponentItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RemoveTraitCommandProperty =
            DependencyProperty.Register(nameof(RemoveTraitCommand), typeof(ICommand), typeof(ComponentItemControl),
                new PropertyMetadata(null));

        public ComponentItemControl()
        {
            InitializeComponent();
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int RarityWeight
        {
            get => (int)GetValue(RarityWeightProperty);
            set => SetValue(RarityWeightProperty, value);
        }

        public object Traits
        {
            get => GetValue(TraitsProperty);
            set => SetValue(TraitsProperty, value);
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public ICommand AddTraitCommand
        {
            get => (ICommand)GetValue(AddTraitCommandProperty);
            set => SetValue(AddTraitCommandProperty, value);
        }

        public ICommand RemoveTraitCommand
        {
            get => (ICommand)GetValue(RemoveTraitCommandProperty);
            set => SetValue(RemoveTraitCommandProperty, value);
        }
    }
}
