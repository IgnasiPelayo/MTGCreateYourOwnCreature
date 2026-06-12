
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Preview
{
    /// <summary>
    /// Interaction logic for TapSymbolControl.xaml.
    /// Represents a dedicated UI component that renders the standard Magic: The Gathering "Tap" symbol.
    /// This control is designed to be dynamically injected into rich text blocks when the {T} token is parsed.
    /// </summary>
    public partial class TapSymbolControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TapSymbolControl"/> class.
        /// Constructs the visual elements defined in the associated XAML markup.
        /// </summary>
        public TapSymbolControl()
        {
            InitializeComponent();
        }
    }
}
