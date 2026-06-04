using System.Windows;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.View.Controls.Preview
{
    public partial class CardPreviewControl : UserControl
    {

        protected const double DescriptionBaseSize = 30;
        protected const double FlavorBaseSize = 38;


        protected bool m_ResizePending = false;


        public CardPreviewControl()
        {
            InitializeComponent();
        }


        protected void ResizeInformation()
        {
            DescriptionTextBlock.FontSize = DescriptionBaseSize;
            FlavorTextBlock.FontSize = FlavorBaseSize;

            UpdateLayout();

            double availableHeight = InformationBorder.ActualHeight;

            double ratio = availableHeight / InformationContainer.ActualHeight;

            if (ratio < 1)
            {
                ResizeTextBlock(DescriptionTextBlock, DescriptionBaseSize, DescriptionBaseSize * ratio);

                if (FlavorTextBlock.Visibility == Visibility.Visible)
                {
                    ResizeTextBlock(FlavorTextBlock, FlavorBaseSize, FlavorBaseSize * ratio);
                }

                UpdateLayout();

                int safetyLimit = 10;

                while (InformationContainer.ActualHeight > availableHeight && safetyLimit-- > 0)
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


        protected void ResizeTextBlock(TextBlock textBlock, double baseSize, double size)
        {
            textBlock.FontSize = size;

            textBlock.LineHeight = size * 0.9;
            textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
        }


        protected void OnInformationSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_ResizePending)
            {
                return;
            }

            m_ResizePending = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ResizeInformation();

                m_ResizePending = false;
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }
}
