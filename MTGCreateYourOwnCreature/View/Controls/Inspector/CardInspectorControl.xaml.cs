using System.Windows.Controls;
using System.Windows.Input;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    public partial class CardInspectorControl : UserControl
    {
        public CardInspectorControl()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
            {
                return;
            }

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta * 0.1));
            e.Handled = true;
        }
    }
}
