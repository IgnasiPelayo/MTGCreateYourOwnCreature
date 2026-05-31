using System.Windows.Input;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.CardsList
{
    public partial class CardListControl : UserControl
    {
        public CardListControl()
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
