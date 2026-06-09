
using System.Windows;
using System.Windows.Input;
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

        /// <summary>
        /// The command that executes when the user requests to delete the currently inspected creature.
        /// </summary>
        public ICommand RemoveCreatureCommand
        {
            get => (ICommand)GetValue(RemoveCreatureCommandProperty);
            set => SetValue(RemoveCreatureCommandProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="RemoveCreatureCommand"/> dependency property.
        /// Allows the parent window to inject the deletion logic from the main application ViewModel.
        /// </summary>
        public static readonly DependencyProperty RemoveCreatureCommandProperty = DependencyProperty.Register(
            nameof(RemoveCreatureCommand), typeof(ICommand), typeof(CardInspectorControl));
    }
}
