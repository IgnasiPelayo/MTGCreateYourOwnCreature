using System.Windows;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Preview
{
    /// <summary>
    /// Interaction logic for CardPreviewControl.xaml.
    /// Represents the UI component that displays a visual preview of the rendered Magic: The Gathering card, 
    /// automatically scaling text to fit within the constrained card frame.
    /// </summary>
    public partial class CardPreviewControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardPreviewControl"/> class.
        /// </summary>
        public CardPreviewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The default, unscaled font size for the rules description text.
        /// </summary>
        protected const double DescriptionBaseSize = 30;

        /// <summary>
        /// The default, unscaled font size for the flavor text.
        /// </summary>
        protected const double FlavorBaseSize = 38;

        /// <summary>
        /// A flag used to prevent redundant or infinite layout update loops when resizing text asynchronously.
        /// </summary>
        protected bool m_ResizePending = false;

        /// <summary>
        /// Dynamically shrinks the font sizes of the description and flavor text blocks 
        /// to ensure they fit within the absolute bounds of the card's information panel.
        /// </summary>
        protected void ResizeInformation()
        {
            DescriptionTextBlock.FontSize = DescriptionBaseSize;
            FlavorTextBlock.FontSize = FlavorBaseSize;

            UpdateLayout();

            double verticalMargins = InformationContainer.Margin.Top + InformationContainer.Margin.Bottom;
            double availableHeight = InformationBorder.ActualHeight - verticalMargins;

            if (availableHeight <= 0)
            {
                return;
            }

            double ratio = availableHeight / InformationContainer.ActualHeight;

            if (ratio < 1)
            {
                ResizeTextBlock(DescriptionTextBlock, DescriptionBaseSize, DescriptionBaseSize * ratio);

                if (FlavorTextBlock.Visibility == Visibility.Visible)
                {
                    ResizeTextBlock(FlavorTextBlock, FlavorBaseSize, FlavorBaseSize * ratio);
                }

                UpdateLayout();

                // In rare cases, fractional pixel rounding or line-wrapping shifts 
                // might prevent the text from fitting perfectly after the first ratio calculation. 
                // We use a strict safety limit of 20 iterations to inch the font size down without risking an infinite loop.
                int safetyLimit = 30;

                while (InformationContainer.ActualHeight > availableHeight && safetyLimit-- > 0 && DescriptionTextBlock.FontSize > 5)
                {
                    ResizeTextBlock(DescriptionTextBlock, DescriptionBaseSize, DescriptionTextBlock.FontSize - 0.5);

                    if (FlavorTextBlock.Visibility == Visibility.Visible)
                    {
                        ResizeTextBlock(FlavorTextBlock, FlavorBaseSize, FlavorTextBlock.FontSize - 0.5);
                    }

                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// Applies a specific font size and proportionally adjusts the line height of a given text block.
        /// </summary>
        /// <param name="textBlock">The text block to modify.</param>
        /// <param name="baseSize">The original target font size (included for reference or future scaling extensions).</param>
        /// <param name="size">The new absolute font size to apply.</param>
        protected void ResizeTextBlock(TextBlock textBlock, double baseSize, double size)
        {
            textBlock.FontSize = size;

            // Tighten the line height slightly based on the font size to mimic traditional MTG card typesetting.
            textBlock.LineHeight = size * 0.9;
            textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
        }

        /// <summary>
        /// Event handler triggered when the text contents or layout constraints of the information panel change.
        /// Queues an asynchronous resize pass to recalculate text fitting.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data detailing the size changes.</param>
        protected void OnInformationSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_ResizePending)
            {
                return;
            }

            m_ResizePending = true;

            // Defer the resize logic to the Render priority dispatcher queue. 
            // This ensures that WPF has completely finished its initial layout pass and that the 
            // ActualHeight properties of the containers are accurately populated before we do math on them.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ResizeInformation();

                m_ResizePending = false;
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }
}
