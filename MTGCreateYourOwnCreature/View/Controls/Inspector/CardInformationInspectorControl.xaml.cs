
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

        /// <summary>
        /// Whether the quick-insert symbol picker (for mana, tap symbols, etc.) should be rendered below the input field.
        /// </summary>
        public bool ShowTagPicker
        {
            get => (bool)GetValue(ShowTagPickerProperty);
            set => SetValue(ShowTagPickerProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="ShowTagPicker"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowTagPickerProperty = DependencyProperty.Register(
            nameof(ShowTagPicker), typeof(bool), typeof(CardInformationInspectorControl), new PropertyMetadata(false));

        /// <summary>
        /// Handles the click event for the dynamically generated symbol picker buttons.
        /// Inserts the corresponding text token (e.g., "{W}", "{T}") directly into the active <see cref="TextBox"/>, 
        /// simulating standard copy/paste behavior by replacing active selections and maintaining cursor flow.
        /// </summary>
        /// <param name="sender">The source <see cref="Button"/> that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        protected void OnTabButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                ValueTextBox.Focus();

                ValueTextBox.SelectedText = tag;

                ValueTextBox.CaretIndex += tag.Length;
                ValueTextBox.SelectionLength = 0;
            }
        }
    }
}
