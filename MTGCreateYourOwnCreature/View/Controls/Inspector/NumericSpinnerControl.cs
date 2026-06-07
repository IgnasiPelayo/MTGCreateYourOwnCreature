
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using MTGCreateYourOwnCreature.ViewModel.Commands;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    /// <summary>
    /// A custom lookless control representing a numeric spinner (up/down control).
    /// Provides increment/decrement commands, boundary validation, and text input sanitation.
    /// Expects a template containing a TextBox named "PART_TextBox".
    /// </summary>
    public class NumericSpinnerControl : Control
    {
        /// <summary>
        /// Initializes the <see cref="NumericSpinnerControl"/> class.
        /// Overrides the default style key to ensure the WPF templating engine looks for the default style in Generic.xaml.
        /// </summary>
        static NumericSpinnerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericSpinnerControl), new FrameworkPropertyMetadata(typeof(NumericSpinnerControl)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericSpinnerControl"/> class.
        /// Prepares the internal commands used by the control's template buttons.
        /// </summary>
        public NumericSpinnerControl()
        {
            m_IncrementCommand = new RelayCommand(_ =>
            {
                if (Value < Maximum)
                {
                    Value++;
                }
            });

            m_DecrementCommand = new RelayCommand(_ =>
            {
                if (Value > Minimum)
                {
                    Value--;
                }
            });
        }

        /// <summary>
        /// The current numeric value of the spinner.
        /// </summary>
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// Configured to bind two-way by default so that UI interactions automatically update the ViewModel.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(NumericSpinnerControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The minimum allowed value for the spinner.
        /// </summary>
        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(int), typeof(NumericSpinnerControl), new PropertyMetadata(0));

        /// <summary>
        /// The maximum allowed value for the spinner.
        /// </summary>
        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(int), typeof(NumericSpinnerControl), new PropertyMetadata(100));

        /// <summary>
        /// The backing field for the <see cref="IncrementCommand"/>.
        /// </summary>
        protected readonly ICommand m_IncrementCommand;

        /// <summary>
        /// The command executed when the user clicks the increment (up) button in the control template.
        /// </summary>
        public ICommand IncrementCommand => m_IncrementCommand;

        /// <summary>
        /// The backing field for the <see cref="DecrementCommand"/>.
        /// </summary>
        protected readonly ICommand m_DecrementCommand;

        /// <summary>
        /// The command executed when the user clicks the decrement (down) button in the control template.
        /// </summary>
        public ICommand DecrementCommand => m_DecrementCommand;

        /// <summary>
        /// The cached reference to the text box defined in the control template.
        /// </summary>
        protected TextBox? m_TextBox;

        /// <summary>
        /// Invoked whenever application code or internal processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// Locates the mandatory template parts and attaches event handlers for input validation.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_TextBox = GetTemplateChild("PART_TextBox") as TextBox;

            if (m_TextBox != null)
            {
                m_TextBox.PreviewTextInput += OnPreviewTextInput;

                DataObject.AddPastingHandler(m_TextBox, OnPaste);
            }
        }

        /// <summary>
        /// Validates direct keyboard input to ensure only valid numeric characters are entered into the text box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data containing the text composition.</param>
        protected void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Block any keystrokes that cannot be parsed as an integer to prevent binding errors.
            e.Handled = !int.TryParse(e.Text, out _);
        }

        /// <summary>
        /// Validates clipboard pasting operations to ensure malicious or malformed text cannot be pasted into the text box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data containing the clipboard payload.</param>
        protected void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            // Reject the paste command entirely if the clipboard doesn't contain text, or if the text cannot be cleanly parsed as an integer.
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string text = (string)e.DataObject.GetData(DataFormats.Text);
            if (!int.TryParse(text, out _))
            {
                e.CancelCommand();
            }
        }
    }
}
