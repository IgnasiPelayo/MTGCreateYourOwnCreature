
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.ViewModel.Cards;
using System.Windows.Input;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    public partial class CardTraitsInspectorControl : UserControl
    {
        public CardTraitsInspectorControl()
        {
            InitializeComponent();
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(CardTraitsInspectorControl), new PropertyMetadata(string.Empty));

        public ObservableCollection<MTGTraitEntryVM> ItemsSource
        {
            get => (ObservableCollection<MTGTraitEntryVM>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(ObservableCollection<MTGTraitEntryVM>), typeof(CardTraitsInspectorControl));

        public string NewValue
        {
            get => (string)GetValue(NewValueProperty);
            set => SetValue(NewValueProperty, value);
        }

        public static readonly DependencyProperty NewValueProperty = DependencyProperty.Register(
            nameof(NewValue), typeof(string), typeof(CardTraitsInspectorControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
            nameof(AddCommand), typeof(ICommand), typeof(CardTraitsInspectorControl));

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            nameof(RemoveCommand), typeof(ICommand), typeof(CardTraitsInspectorControl));
    }
}
