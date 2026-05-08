using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MTGCreateYourOwnCreature.View.Controls
{
    public partial class CardListItemControl : UserControl
    {
        public CardListItemControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsNameReadOnlyProperty = DependencyProperty.Register(
            nameof(IsNameReadOnly), typeof(bool), typeof(CardListItemControl), new PropertyMetadata(true));

        public bool IsNameReadOnly
        {
            get => (bool)GetValue(IsNameReadOnlyProperty);
            set => SetValue(IsNameReadOnlyProperty, value);
        }

    }
}
