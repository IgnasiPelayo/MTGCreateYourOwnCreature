using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Preview
{
    public partial class CardPreviewFrameControl : UserControl
    {
        public CardPreviewFrameControl()
        {
            InitializeComponent();
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(CardPreviewFrameControl));
    }
}
