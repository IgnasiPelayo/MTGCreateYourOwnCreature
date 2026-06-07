
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Mana
{
    /// <summary>
    /// Interaction logic for ManaSymbolControl.xaml.
    /// Represents a UI component that renders a single Magic: The Gathering mana symbol, 
    /// combining a visual background color with an inner text or icon value.
    /// </summary>
    public partial class ManaSymbolControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManaSymbolControl"/> class.
        /// </summary>
        public ManaSymbolControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The string value displayed inside the mana symbol (e.g., "1", "2", "X").
        /// </summary>
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// Enables XAML data binding for the inner text or character of the mana symbol.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(ManaSymbolControl), new PropertyMetadata(null));

        /// <summary>
        /// The brush used to paint the visual background of the mana symbol.
        /// </summary>
        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Brush"/> dependency property.
        /// Enables XAML data binding to dynamically color the symbol based on its specific mana type.
        /// </summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush), typeof(Brush), typeof(ManaSymbolControl), new PropertyMetadata(null));
    }
}
