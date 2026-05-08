using MTGCreateYourOwnCreature.Model;
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

        public MTGCreatureCard.MTGCreatureMana Mana
        {
            get => (MTGCreatureCard.MTGCreatureMana)GetValue(ManaProperty);
            set => SetValue(ManaProperty, value);
        }

        public static readonly DependencyProperty ManaProperty = DependencyProperty.Register(
            nameof(Mana), typeof(MTGCreatureCard.MTGCreatureMana), typeof(ManaCostControl), new PropertyMetadata(null));
    }
}
