using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls
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
