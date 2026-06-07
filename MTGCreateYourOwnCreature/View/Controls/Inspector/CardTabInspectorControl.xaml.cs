
using System.Windows.Input;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Inspector
{
    /// <summary>
    /// Interaction logic for CardTabInspectorControl.xaml.
    /// Represents the UI component within the card inspector used to display and navigate between different categorized tabs of a creature card's properties.
    /// </summary>
    public partial class CardTabInspectorControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardTabInspectorControl"/> class.
        /// </summary>
        public CardTabInspectorControl()
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
            // We manually dampen the scroll distance by 90% (0.1 multiplier) for smoother navigation,
            // and explicitly mark the event as handled to prevent the default WPF scroll behavior from doubling up.
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta * 0.1));
            e.Handled = true;
        }
    }
}
