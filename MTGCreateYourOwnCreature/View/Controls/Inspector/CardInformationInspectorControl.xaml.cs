
using System.Windows;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    /// <summary>
    /// Interaction logic for CardInformationInspectorControl.xaml.
    /// Represents a reusable UI component within the card inspector used for viewing and editing multi-line text fields, such as rules descriptions or flavor text.
    /// </summary>
    public partial class CardInformationInspectorControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardInformationInspectorControl"/> class.
        /// </summary>
        public CardInformationInspectorControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The text label displayed alongside this information input field.
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
            nameof(Label), typeof(string), typeof(CardInformationInspectorControl), new PropertyMetadata(string.Empty));
    }
}
