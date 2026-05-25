using System.Windows.Input;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Modals
{
    public partial class ParentCardPickerModalControl : UserControl
    {
        public ParentCardPickerModalControl()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
            {
                return;
            }

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta * 0.5));
            e.Handled = true;
        }
    }
}
