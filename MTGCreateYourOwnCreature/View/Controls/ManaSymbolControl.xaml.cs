using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls
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

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(ManaSymbolControl), new PropertyMetadata(null));
    }
}
