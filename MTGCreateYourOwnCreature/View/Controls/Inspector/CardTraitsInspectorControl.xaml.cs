
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.ViewModel.Cards;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    /// <summary>
    /// Interaction logic for CardTraitsInspectorControl.xaml.
    /// Represents a reusable UI component within the card inspector used to view, add, and remove 
    /// a collection of text-based traits (such as tags or gameplay keywords).
    /// </summary>
    public partial class CardTraitsInspectorControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardTraitsInspectorControl"/> class.
        /// </summary>
        public CardTraitsInspectorControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The text label displayed alongside this traits control.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(CardTraitsInspectorControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// The observable collection of trait entries bound to the control's list view.
        /// </summary>
        public ObservableCollection<MTGTraitEntryVM> ItemsSource
        {
            get => (ObservableCollection<MTGTraitEntryVM>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(ObservableCollection<MTGTraitEntryVM>), typeof(CardTraitsInspectorControl));

        /// <summary>
        /// The current text entered into the new trait input field.
        /// </summary>
        public string NewValue
        {
            get => (string)GetValue(NewValueProperty);
            set => SetValue(NewValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="NewValue"/> dependency property.
        /// Configured to bind two-way by default to ensure input text correctly pushes back to the ViewModel.
        /// </summary>
        public static readonly DependencyProperty NewValueProperty = DependencyProperty.Register(
            nameof(NewValue), typeof(string), typeof(CardTraitsInspectorControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The command executed when the user submits a new trait to be added to the collection.
        /// </summary>
        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="AddCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
            nameof(AddCommand), typeof(ICommand), typeof(CardTraitsInspectorControl));

        /// <summary>
        /// The command executed when the user removes a specific trait from the collection.
        /// </summary>
        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="RemoveCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            nameof(RemoveCommand), typeof(ICommand), typeof(CardTraitsInspectorControl));
    }
}
