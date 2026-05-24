using System.Windows.Media;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;

namespace MTGCreateYourOwnCreature.Rendering
{
    public static class ManaRenderService
    {
        public static Dictionary<ManaType, Brush?> ms_ManaBrushes = new Dictionary<ManaType, Brush?>()
        {
            { ManaType.Colorless, Brushes.LightGray },
            { ManaType.White, App.Current.TryFindResource("WhiteManaBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BlueManaBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackManaBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedManaBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenManaBrush") as Brush },
        };


        public static MTGManaSymbol CreatePreviewSymbol(ManaType manaType)
        {
            return new MTGManaSymbol(manaType == ManaType.Colorless ? "X" : "", ms_ManaBrushes[manaType]);
        }


        public static IReadOnlyList<MTGManaSymbol> CreateSymbols(IReadOnlyDictionary<ManaType, int> mana, bool inherited = false)
        {
            List<MTGManaSymbol> symbols = new List<MTGManaSymbol>();

            foreach (KeyValuePair<ManaType, int> manaEntry in mana)
            {
                if (manaEntry.Value <= 0)
                {
                    continue;
                }

                Brush? brush = ms_ManaBrushes[manaEntry.Key];

                if (manaEntry.Key == ManaType.Colorless)
                {
                    symbols.Add(new MTGManaSymbol(manaEntry.Value.ToString(), brush, inherited));
                }
                else
                {
                    for (int i = 0; i < manaEntry.Value; ++i)
                    {
                        symbols.Add(new MTGManaSymbol("", brush, inherited));
                    }
                }
            }

            return symbols;
        }
    }
}
