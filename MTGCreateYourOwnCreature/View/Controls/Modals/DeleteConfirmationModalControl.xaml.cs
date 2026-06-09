
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Modals
{
    /// <summary>
    /// Interaction logic for DeleteConfirmationModalControl.xaml.
    /// Represents a modal UI component overlay that forces the user to confirm the deletion of a creature card, 
    /// especially when that card acts as a parent to other inherited creatures.
    /// </summary>
    public partial class DeleteConfirmationModalControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteConfirmationModalControl"/> class.
        /// </summary>
        public DeleteConfirmationModalControl()
        {
            InitializeComponent();
        }
    }
}