using System.Windows;
using System.Windows.Controls;

using MTGCreateYourOwnCreature.Model.Mana;

namespace MTGCreateYourOwnCreature.View.Controls.Mana
{
    public partial class ManaCostControl : UserControl
    {
        public ManaCostControl()
        {
            InitializeComponent();
        }

        public Dictionary<ManaType, int> Mana
        {
            get => (Dictionary<ManaType, int>)GetValue(ManaProperty);
            set => SetValue(ManaProperty, value);
        }

        public static readonly DependencyProperty ManaProperty = DependencyProperty.Register(
            nameof(Mana), typeof(Dictionary<ManaType, int>), typeof(ManaCostControl), new PropertyMetadata(null));
    }
}
