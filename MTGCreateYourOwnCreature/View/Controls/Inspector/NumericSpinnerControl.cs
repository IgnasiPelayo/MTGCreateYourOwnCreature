using MTGCreateYourOwnCreature.ViewModel.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    public class NumericSpinnerControl : Control
    {
        static NumericSpinnerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericSpinnerControl), new FrameworkPropertyMetadata(typeof(NumericSpinnerControl)));
        }

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

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(NumericSpinnerControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }


        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(int), typeof(NumericSpinnerControl), new PropertyMetadata(0));

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(int), typeof(NumericSpinnerControl), new PropertyMetadata(100));

        protected readonly ICommand m_IncrementCommand;
        public ICommand IncrementCommand => m_IncrementCommand;

        protected readonly ICommand m_DecrementCommand;
        public ICommand DecrementCommand => m_DecrementCommand;

        protected TextBox? m_TextBox;

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

        protected void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        protected void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
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
