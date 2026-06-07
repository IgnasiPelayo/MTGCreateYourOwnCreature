
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Preview
{
    /// <summary>
    /// Interaction logic for CardPreviewFrameControl.xaml.
    /// Represents the UI component responsible for rendering the decorative outer frame or background border of the card preview.
    /// </summary>
    public partial class CardPreviewFrameControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardPreviewFrameControl"/> class.
        /// </summary>
        public CardPreviewFrameControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The brush used to paint the fill area of the card frame.
        /// </summary>
        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property.
        /// Enables XAML data binding to dynamically change the card frame's color based on its mana identity or type.
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(CardPreviewFrameControl));
    }
}
