using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Mana
{
    public partial class ManaSymbolControl : UserControl
    {
        public ManaSymbolControl()
        {
            InitializeComponent();
        }

        public String Value
        {
            get => (String)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(String), typeof(ManaSymbolControl), new PropertyMetadata(null));

        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush), typeof(Brush), typeof(ManaSymbolControl), new PropertyMetadata(null));
    }
}
