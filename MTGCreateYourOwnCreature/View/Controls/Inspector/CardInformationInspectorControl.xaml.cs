using System.Windows;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    public partial class CardInformationInspectorControl : UserControl
    {
        public CardInformationInspectorControl()
        {
            InitializeComponent();
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(CardInformationInspectorControl), new PropertyMetadata(string.Empty));
    }
}
