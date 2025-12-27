using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Game.ContentBuilder.Converters;

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
                new FrameworkPropertyMetadata(50, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRarityWeightChanged));

        public static readonly DependencyProperty TraitsProperty =
            DependencyProperty.Register(nameof(Traits), typeof(object), typeof(ComponentItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(ComponentItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty AddTraitCommandProperty =
            DependencyProperty.Register(nameof(AddTraitCommand), typeof(ICommand), typeof(ComponentItemControl),
                new PropertyMetadata(null));

        public ComponentItemControl()
        {
            InitializeComponent();
            UpdateRarityBadge(RarityWeight);
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

        private static void OnRarityWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComponentItemControl control && e.NewValue is int weight)
            {
                control.UpdateRarityBadge(weight);
            }
        }

        private void UpdateRarityBadge(int weight)
        {
            var colorConverter = new RarityWeightToColorConverter();
            var nameConverter = new RarityWeightToNameConverter();

            var color = colorConverter.Convert(weight, typeof(System.Windows.Media.Brush), null!, System.Globalization.CultureInfo.CurrentCulture);
            var name = nameConverter.Convert(weight, typeof(string), null!, System.Globalization.CultureInfo.CurrentCulture);

            if (color is System.Windows.Media.Brush brush)
            {
                WeightBadge.Background = brush;
            }

            if (name is string rarityName)
            {
                WeightName.Text = rarityName;
            }
        }
    }
}
