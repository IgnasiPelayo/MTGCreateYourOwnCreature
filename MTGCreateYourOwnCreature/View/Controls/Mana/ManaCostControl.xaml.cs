
using System.Windows;
using System.Windows.Controls;

using MTGCreateYourOwnCreature.ViewModel.Cards;

namespace MTGCreateYourOwnCreature.View.Controls.Mana
{
    /// <summary>
    /// Interaction logic for ManaCostControl.xaml.
    /// Represents a UI component used to visually display a sequence or collection of Magic: The Gathering mana symbols.
    /// </summary>
    public partial class ManaCostControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManaCostControl"/> class.
        /// </summary>
        public ManaCostControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The collection of mana symbols to be rendered by this control.
        /// </summary>
        public IEnumerable<MTGManaSymbolVM> ItemsSource
        {
            get => (IEnumerable<MTGManaSymbolVM>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// Enables XAML data binding to a ViewModel's collection of generated mana symbols.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable<MTGManaSymbolVM>), typeof(ManaCostControl));
    }
}
