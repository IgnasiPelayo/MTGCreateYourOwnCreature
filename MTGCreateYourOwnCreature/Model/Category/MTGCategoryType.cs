
using System.ComponentModel.DataAnnotations;

namespace MTGCreateYourOwnCreature.Model.Category
{
    /// <summary>
    /// Defines the primary type line categories available for a creature card in the editor.
    /// Uses DataAnnotations to provide formatted, human-readable strings for UI binding.
    /// Note: Binding these Display names in WPF XAML requires an IValueConverter or a custom EnumHelper extension.
    /// </summary>
    public enum CategoryType
    {
        /// <summary>
        /// Represents an unassigned or default initialization state. 
        /// Typically filtered out of UI dropdowns or used as a placeholder.
        /// </summary>
        None,

        Creature,

        [Display(Name = "Legendary Creature")]
        LegendaryCreature,

        [Display(Name = "Artifact Creature")]
        ArtifactCreature,

        [Display(Name = "Legendary Artifact Creature")]
        LegendaryArtifactCreature,

        [Display(Name = "Enchantment Creature")]
        EnchantmentCreature,

        [Display(Name = "Legendary Enchantment Creature")]
        LegendaryEnchantmentCreature
    }
}
