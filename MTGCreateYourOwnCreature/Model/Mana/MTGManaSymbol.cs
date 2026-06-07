
using System.Windows.Media;

namespace MTGCreateYourOwnCreature.Model
{
    /// <summary>
    /// A bindable object representing a single mana symbol for rendering in the WPF UI.
    /// Encapsulates both the raw data and its visual styling.
    /// </summary>
    public class MTGManaSymbol
    {
        /// <summary>
        /// The text or number displayed in the symbol (e.g. "X", "1", "2"...).
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The WPF brush used to render the symbol's specific color identity. 
        /// </summary>
        public Brush? Brush { get; set; }

        /// <summary>
        /// Flags whether this symbol was inherited from a ParentCreatureCard.
        /// Used to visually distinguish inherited costs.
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// Initializes a new bindable mana symbol.
        /// </summary>
        /// <param name="value">The raw string representation of the mana.</param>
        /// <param name="brush">The associated WPF color/brush, or null for default styling.</param>
        /// <param name="isInherited">True if the symbol is derived from a parent card.</param>
        public MTGManaSymbol(string value, Brush? brush, bool isInherited = false)
        {
            Value = value;
            Brush = brush;
            IsInherited = isInherited;
        }
    }
}
