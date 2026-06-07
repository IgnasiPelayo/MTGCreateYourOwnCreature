
namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    /// <summary>
    /// ViewModel representing a single trait to be displayed in the UI.
    /// Encapsulates the string value and its inheritance state.
    /// </summary>
    public class MTGTraitEntryVM
    {
        /// <summary>
        /// The underlying string value of the trait.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Whether this trait is inherited from a parent card.
        /// </summary>
        public bool IsInherited { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MTGTraitEntryVM"/> class.
        /// </summary>
        /// <param name="value">The underlying text string of the trait.</param>
        /// <param name="isInherited">True if this trait originates from a parent card; otherwise, false.</param>
        public MTGTraitEntryVM(string value, bool isInherited)
        {
            Value = value;
            IsInherited = isInherited;
        }
    }
}
