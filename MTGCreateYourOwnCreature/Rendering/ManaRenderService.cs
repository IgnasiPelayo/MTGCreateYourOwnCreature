
using System.Windows.Media;

using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.ViewModel.Cards;

namespace MTGCreateYourOwnCreature.Rendering
{
    /// <summary>
    /// Translates raw mana data from the model into UI-ready bindable objects.
    /// Acts as the bridge between the data layer and the visual representation in the editor.
    /// </summary>
    public static class ManaRenderService
    {
        /// <summary>
        /// Caches the brushes used for each mana type.
        /// Requires matching Brush resources (e.g., SolidColorBrush or ImageBrush) defined in App.xaml or a merged dictionary.
        /// </summary>
        public static readonly Dictionary<ManaType, Brush> ms_ManaBrushes = new Dictionary<ManaType, Brush>()
        {
            { ManaType.Generic, Brushes.LightGray },
            { ManaType.White, FindBrush("WhiteManaBrush") },
            { ManaType.Blue, FindBrush("BlueManaBrush") },
            { ManaType.Black, FindBrush("BlackManaBrush") },
            { ManaType.Red, FindBrush("RedManaBrush") },
            { ManaType.Green, FindBrush("GreenManaBrush") },
        };

        /// <summary>
        /// Creates a static preview symbol, used as a label for one mana row in the inspector.
        /// </summary>
        /// <param name="manaType">The type of mana to generate a preview for.</param>
        /// <returns>A single MTGManaSymbolVM, using "X" for Generic mana to denote a variable quantity.</returns>
        public static MTGManaSymbolVM CreatePreviewSymbol(ManaType manaType)
        {
            return new MTGManaSymbolVM(manaType == ManaType.Generic ? "X" : "", ms_ManaBrushes[manaType]);
        }

        /// <summary>
        /// Translates a raw mana cost dictionary into an ordered list of bindable visual symbols.
        /// Implements standard MTG formatting: Generic mana aggregates into a single numeric symbol, 
        /// while colored mana generates distinct, individual symbols for each pip.
        /// </summary>
        /// <param name="mana">The raw dictionary mapping mana types to their required quantities.</param>
        /// <param name="inherited">Flags the output symbols as inherited so the XAML can apply specialized UI styling.</param>
        /// <returns>A flat list of symbols ready to be bound to a WPF ItemsControl.</returns>
        public static IReadOnlyList<MTGManaSymbolVM> CreateSymbols(IReadOnlyDictionary<ManaType, int> mana, bool inherited = false)
        {
            List<MTGManaSymbolVM> symbols = new List<MTGManaSymbolVM>();

            foreach (KeyValuePair<ManaType, int> manaEntry in mana)
            {
                // Skip empty mana requirements entirely so we don't render blank spaces in the UI.
                if (manaEntry.Value <= 0)
                {
                    continue;
                }

                Brush? brush = ms_ManaBrushes[manaEntry.Key];

                if (manaEntry.Key == ManaType.Generic)
                {
                    // Generic mana is grouped into a single circle with a number (e.g., "(3)").
                    symbols.Add(new MTGManaSymbolVM(manaEntry.Value.ToString(), brush, inherited));
                }
                else
                {
                    // Colored mana is split into individual symbols (e.g., "(W)(W)(W)").
                    // We pass an empty string because the visual brush handles the rendering.
                    for (int i = 0; i < manaEntry.Value; ++i)
                    {
                        symbols.Add(new MTGManaSymbolVM("", brush, inherited));
                    }
                }
            }

            return symbols;
        }

        /// <summary>
        /// Safely retrieves a brush from the WPF Application resources without crashing if the resource is missing.
        /// </summary>
        /// <param name="resourceKey">The x:Key of the brush resource to locate.</param>
        /// <returns>The resolved Brush, or Brushes.Pink as a safe fallback.</returns>
        public static Brush FindBrush(string resourceKey)
        {
            return App.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Pink;
        }
    }
}
