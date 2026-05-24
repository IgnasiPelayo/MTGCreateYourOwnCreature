using System.Windows;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    public partial class CardInheritedNumericInspectorControl : UserControl
    {
        public CardInheritedNumericInspectorControl()
        {
            InitializeComponent();
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(string.Empty));


        public int LocalValue
        {
            get => (int)GetValue(LocalValueProperty);
            set => SetValue(LocalValueProperty, value);
        }

        public static readonly DependencyProperty LocalValueProperty = DependencyProperty.Register(
            nameof(LocalValue), typeof(int), typeof(CardInheritedNumericInspectorControl), 
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public int InheritedValue
        {
            get => (int)GetValue(InheritedValueProperty);
            set => SetValue(InheritedValueProperty, value);
        }

        public static readonly DependencyProperty InheritedValueProperty = DependencyProperty.Register(
            nameof(InheritedValue), typeof(int), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(0));

        public int TotalValue
        {
            get => (int)GetValue(TotalValueProperty);
            set => SetValue(TotalValueProperty, value);
        }

        public static readonly DependencyProperty TotalValueProperty = DependencyProperty.Register(
            nameof(TotalValue), typeof(int), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(0));

        public bool HasInheritance
        {
            get => (bool)GetValue(HasInheritanceProperty);
            set => SetValue(HasInheritanceProperty, value);
        }

        public static readonly DependencyProperty HasInheritanceProperty = DependencyProperty.Register(
            nameof(HasInheritance), typeof(bool), typeof(CardInheritedNumericInspectorControl), new PropertyMetadata(false));
    }
}
