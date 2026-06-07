
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    /// <summary>
    /// Interaction logic for CardInspectorControl.xaml.
    /// Represents the main UI container for editing a selected creature card's properties, 
    /// hosting various sub-controls for mana, stats, categories, and text fields.
    /// </summary>
    public partial class CardInspectorControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardInspectorControl"/> class.
        /// </summary>
        public CardInspectorControl()
        {
            InitializeComponent();
        }
    }
}
