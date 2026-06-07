
using System.Windows.Input;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Modals
{
    /// <summary>
    /// Interaction logic for ParentCardPickerModalControl.xaml.
    /// Represents a modal UI component overlay that allows the user to browse and select a parent card to inherit properties from.
    /// </summary>
    public partial class ParentCardPickerModalControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParentCardPickerModalControl"/> class.
        /// </summary>
        public ParentCardPickerModalControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Intercepts the mouse wheel scroll event on the internal scroll viewer to apply a custom scrolling speed.
        /// </summary>
        /// <param name="sender">The source of the event, expected to be a <see cref="ScrollViewer"/>.</param>
        /// <param name="e">The event data containing the scroll delta.</param>
        private void ScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
            {
                return;
            }

            // The default scroll speed on this specific items control is too aggressive.
            // We manually dampen the scroll distance by 50% (0.5 multiplier) for smoother navigation,
            // and explicitly mark the event as handled to prevent the default WPF scroll behavior from doubling up.
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta * 0.5));
            e.Handled = true;
        }
    }
}
