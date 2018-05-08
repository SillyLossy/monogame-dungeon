using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Characters;

namespace SpecialAdventure.Core.Entities.Items
{
    public class WeaponFactory : ItemFactory
    {
        public IntRange Damage { get; set; }
        public Skill GovernedSkill { get; set; }
        public int ActionPointsRequired { get; set; }

        public override Item Construct()
        {
            return new Weapon(Name, Weight, BasePrice, Damage, GovernedSkill, ActionPointsRequired, SpriteId);
        }
    }
}