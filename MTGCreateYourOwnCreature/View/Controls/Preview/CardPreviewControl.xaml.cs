
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Text.RegularExpressions;

using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.ViewModel.Cards;
using MTGCreateYourOwnCreature.View.Controls.Mana;

namespace MTGCreateYourOwnCreature.View.Controls.Preview
{
    /// <summary>
    /// Interaction logic for CardPreviewControl.xaml.
    /// Represents the UI component that displays a visual preview of the rendered Magic: The Gathering card, 
    /// parsing inline symbols (like {T} or {W}) and automatically scaling them to fit the card frame.
    /// </summary>
    public partial class CardPreviewControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardPreviewControl"/> class.
        /// </summary>
        public CardPreviewControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
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
        /// A reference to the currently bound card to listen for text changes.
        /// </summary>
        protected MTGCreatureCardVM? m_BoundCard;

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

            textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
        }

        /// <summary>
        /// Parses the bound card's description text, identifies formatting tokens (e.g., {W}, {T}, {Name}), 
        /// and constructs a rich text flow of alternating text runs and visual UI elements.
        /// </summary>
        private void RenderDescription()
        {
            DescriptionTextBlock.Inlines.Clear();

            if (m_BoundCard == null)
            {
                return;
            }

            // Inject resolved keywords at the very beginning of the description block.
            if (!string.IsNullOrEmpty(m_BoundCard.ResolvedKeywords))
            {
                DescriptionTextBlock.Inlines.Add(new Run(m_BoundCard.ResolvedKeywords));
            }

            string text = m_BoundCard.Description?.ResolvedValue ?? string.Empty;

            // Tokenize the description string looking for content wrapped in curly braces.
            Regex regex = new Regex(@"\{([^}]+)\}");
            int lastIndex = 0;

            foreach (Match match in regex.Matches(text))
            {
                // Add any standard text that occurred before this match.
                if (match.Index > lastIndex)
                {
                    DescriptionTextBlock.Inlines.Add(new Run(text.Substring(lastIndex, match.Index - lastIndex)));
                }

                string token = match.Groups[1].Value;

                // Resolve the matched token into a visual UI element or dynamic text and inject it.
                DescriptionTextBlock.Inlines.Add(ResolveToken(token, m_BoundCard));

                lastIndex = match.Index + match.Length;
            }

            // Append any remaining standard text that occurred after the final matched token.
            if (lastIndex < text.Length)
            {
                DescriptionTextBlock.Inlines.Add(new Run(text.Substring(lastIndex)));
            }
        }

        /// <summary>
        /// Maps an extracted string token (e.g., "W", "T", "Name") to its corresponding visual <see cref="Inline"/> representation.
        /// Integrates with the <see cref="Rendering.ManaRenderService"/> to fetch appropriate visual brushes.
        /// </summary>
        /// <param name="token">The raw string extracted from the curly braces.</param>
        /// <param name="card">The currently bound card, used to resolve dynamic text like the card's name.</param>
        /// <returns>An <see cref="Inline"/> element containing either standard text or an encapsulated UserControl.</returns>
        private Inline ResolveToken(string token, MTGCreatureCardVM card)
        {
            string upperToken = token.ToUpper();

            switch (upperToken)
            {
                case "NAME":
                    return new Run(card.Name);

                case "T":
                    return CreateSymbolInline(new TapSymbolControl());

                case "W": return CreateManaInline("", ManaType.White);
                case "U": return CreateManaInline("", ManaType.Blue);
                case "B": return CreateManaInline("", ManaType.Black);
                case "R": return CreateManaInline("", ManaType.Red);
                case "G": return CreateManaInline("", ManaType.Green);

                case "X": return CreateManaInline("X", ManaType.Generic);
                
                default:
                    // Support numeric generic mana costs like {1}, {2}, or {15}.
                    if (int.TryParse(upperToken, out _))
                    {
                        return CreateManaInline(upperToken, ManaType.Generic);
                    }

                    // If the token is unrecognized, render it exactly as it was typed.
                    return new Run($"{{{token}}}");
            }
        }

        /// <summary>
        /// Helper method to initialize a <see cref="ManaSymbolControl"/> with the correct brush and inner value.
        /// </summary>
        /// <param name="innerValue">The text displayed inside the symbol (empty for colored mana, numeric/X for generic).</param>
        /// <param name="type">The type of mana, used to look up the correct visual brush.</param>
        /// <returns>An inline container holding the rendered mana symbol.</returns>
        private Inline CreateManaInline(string innerValue, ManaType type)
        {
            ManaSymbolControl manaControl = new ManaSymbolControl
            {
                Value = innerValue,

                Brush = Rendering.ManaRenderService.ms_ManaBrushes[type]
            };

            return CreateSymbolInline(manaControl);
        }

        /// <summary>
        /// Wraps a generated UserControl in a <see cref="Viewbox"/> and <see cref="InlineUIContainer"/>, dynamically binding 
        /// its size to the parent TextBlock's Font Size to guarantee perfect scaling during layout resize loops.
        /// </summary>
        /// <param name="symbolControl">The raw UI control to encapsulate.</param>
        /// <returns>A fully bound and scaled inline element ready for injection into a TextBlock.</returns>
        private Inline CreateSymbolInline(UserControl symbolControl)
        {
            Viewbox viewBox = new Viewbox
            {
                Child = symbolControl,
                Stretch = Stretch.Uniform
            };

            // Bind explicitly to the FontSize of the ancestor TextBlock so symbols scale down alongside the text.
            Binding sizeBinding = new Binding(nameof(TextBlock.FontSize))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TextBlock), 1)
            };

            viewBox.SetBinding(FrameworkElement.WidthProperty, sizeBinding);
            viewBox.SetBinding(FrameworkElement.HeightProperty, sizeBinding);

            viewBox.Margin = new Thickness(2, -4, 2, -2);

            return new InlineUIContainer(viewBox) { BaselineAlignment = BaselineAlignment.Center };
        }

        /// <summary>
        /// Manages event subscriptions when the active DataContext changes, ensuring old listeners are cleaned up to prevent memory leaks.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_BoundCard != null)
            {
                m_BoundCard.PropertyChanged -= OnCardPropertyChanged;
            }

            m_BoundCard = e.NewValue as MTGCreatureCardVM;

            if (m_BoundCard != null)
            {
                m_BoundCard.PropertyChanged += OnCardPropertyChanged;
            }

            // Immediately force a render pass when a new card is selected.
            RenderDescription();
        }

        /// <summary>
        /// Listens for property changes on the active card model. If fields affecting the visual text layout 
        /// are modified, this triggers a re-parsing and re-rendering of the rich text UI.
        /// </summary>
        private void OnCardPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Description" || e.PropertyName == "Name" || e.PropertyName == "ResolvedKeywords")
            {
                RenderDescription();
            }
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
