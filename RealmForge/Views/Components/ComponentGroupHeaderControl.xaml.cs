using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Game.ContentBuilder.Views.Components
{
    /// <summary>
    /// Interaction logic for ComponentGroupHeaderControl.xaml
    /// Displays the header for a component group with key, count, and action buttons.
    /// </summary>
    public partial class ComponentGroupHeaderControl : UserControl
    {
        public static readonly DependencyProperty GroupKeyProperty =
            DependencyProperty.Register(nameof(GroupKey), typeof(string), typeof(ComponentGroupHeaderControl),
                new PropertyMetadata(string.Empty, OnGroupKeyChanged));

        public static readonly DependencyProperty ItemCountProperty =
            DependencyProperty.Register(nameof(ItemCount), typeof(int), typeof(ComponentGroupHeaderControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(ComponentGroupHeaderControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(ComponentGroupHeaderControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsBaseGroupProperty =
            DependencyProperty.Register(nameof(IsBaseGroup), typeof(bool), typeof(ComponentGroupHeaderControl),
                new PropertyMetadata(false, OnIsBaseGroupChanged));

        public ComponentGroupHeaderControl()
        {
            InitializeComponent();
            UpdateButtonVisibility();
        }

        public string GroupKey
        {
            get => (string)GetValue(GroupKeyProperty);
            set => SetValue(GroupKeyProperty, value);
        }

        public int ItemCount
        {
            get => (int)GetValue(ItemCountProperty);
            set => SetValue(ItemCountProperty, value);
        }

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public bool IsBaseGroup
        {
            get => (bool)GetValue(IsBaseGroupProperty);
            set => SetValue(IsBaseGroupProperty, value);
        }

        private static void OnGroupKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComponentGroupHeaderControl control)
            {
                control.UpdateButtonVisibility();
            }
        }

        private static void OnIsBaseGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComponentGroupHeaderControl control)
            {
                control.UpdateButtonVisibility();
            }
        }

        private void UpdateButtonVisibility()
        {
            // Hide Add and Delete buttons for the "base" group
            bool isBase = IsBaseGroup || string.Equals(GroupKey, "base", System.StringComparison.OrdinalIgnoreCase);
            
            AddButton.Visibility = isBase ? Visibility.Collapsed : Visibility.Visible;
            DeleteButton.Visibility = isBase ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
