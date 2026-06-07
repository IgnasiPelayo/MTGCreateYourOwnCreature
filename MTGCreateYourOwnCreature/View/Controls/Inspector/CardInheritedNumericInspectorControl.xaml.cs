
using System.Windows;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    /// <summary>
    /// Interaction logic for CardInheritedNumericInspectorControl.xaml.
    /// Represents a reusable UI component within the card inspector used for viewing and editing numeric values 
    /// that support an inheritance model (such as Power or Toughness).
    /// </summary>
    public partial class CardInheritedNumericInspectorControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardInheritedNumericInspectorControl"/> class.
        /// </summary>
        public CardInheritedNumericInspectorControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The text label displayed alongside this numeric input field.
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
            nameof(Label), typeof(string), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// The explicit local numeric value defined for this specific card.
        /// </summary>
        public int LocalValue
        {
            get => (int)GetValue(LocalValueProperty);
            set => SetValue(LocalValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="LocalValue"/> dependency property.
        /// Configured to bind two-way by default to ensure UI spinner changes propagate back to the ViewModel.
        /// </summary>
        public static readonly DependencyProperty LocalValueProperty = DependencyProperty.Register(
            nameof(LocalValue), typeof(int), typeof(CardInheritedNumericInspectorControl), 
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The numeric value inherited from the parent card.
        /// </summary>
        public int InheritedValue
        {
            get => (int)GetValue(InheritedValueProperty);
            set => SetValue(InheritedValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="InheritedValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InheritedValueProperty = DependencyProperty.Register(
            nameof(InheritedValue), typeof(int), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(0));

        /// <summary>
        /// The total resolved numeric value (LocalValue + InheritedValue).
        /// </summary>
        public int TotalValue
        {
            get => (int)GetValue(TotalValueProperty);
            set => SetValue(TotalValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TotalValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TotalValueProperty = DependencyProperty.Register(
            nameof(TotalValue), typeof(int), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(0));

        /// <summary>
        /// Whether this card inherits values from a parent card.
        /// Typically used to toggle the visibility of the inheritance UI indicators.
        /// </summary>
        public bool HasInheritance
        {
            get => (bool)GetValue(HasInheritanceProperty);
            set => SetValue(HasInheritanceProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="HasInheritance"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasInheritanceProperty = DependencyProperty.Register(
            nameof(HasInheritance), typeof(bool), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(false));
    }
}
