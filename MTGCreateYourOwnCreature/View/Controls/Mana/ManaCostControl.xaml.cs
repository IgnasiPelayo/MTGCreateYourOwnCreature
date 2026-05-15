using System.Windows;
using System.Windows.Controls;

using MTGCreateYourOwnCreature.Model;

namespace MTGCreateYourOwnCreature.View.Controls.Mana
{
    public partial class ManaCostControl : UserControl
    {
        public ManaCostControl()
        {
            InitializeComponent();
        }

        public IEnumerable<MTGManaSymbol> ItemsSource
        {
            get => (IEnumerable<MTGManaSymbol>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable<MTGManaSymbol>), typeof(ManaCostControl));
    }
}
