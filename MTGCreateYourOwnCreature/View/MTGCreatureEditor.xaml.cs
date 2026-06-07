using System.Windows;

namespace MTGCreateYourOwnCreature.View
{
    /// <summary>
    /// Interaction logic for MTGCreatureEditor.xaml.
    /// Represents the primary application window that serves as the root layout container for the creature editor UI.
    /// Hosts the card selection list, the real-time visual card preview, and the right-hand property inspector.
    /// </summary>
    public partial class MTGCreatureEditor : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MTGCreatureEditor"/> class.
        /// </summary>
        public MTGCreatureEditor()
        {
            InitializeComponent();
        }
    }
}
