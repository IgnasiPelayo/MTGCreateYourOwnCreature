
using System.ComponentModel.DataAnnotations;

namespace MTGCreateYourOwnCreature.Model.Category
{
    public enum CategoryType
    {
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
