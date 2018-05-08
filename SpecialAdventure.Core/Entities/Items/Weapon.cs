using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Characters;

namespace SpecialAdventure.Core.Entities.Items
{
    public class Weapon : Item
    {
        public IntRange Damage { get; set; }
        public Skill GovernedSkill { get; set; }
        public int ActionPointsRequired { get; set; }
        
        public static Weapon UnarmedWeapon(int damage)
        {
            const int UnarmedActionPointsRequirement = 3;
            return new Weapon("", 0, 0, new IntRange(damage), Skill.Unarmed, UnarmedActionPointsRequirement, 0);
        }

        public Weapon(string name, double weight, int basePrice, IntRange damage, Skill governedSkill, int actionPointsRequired, int spriteId) : base(name, weight, basePrice, spriteId)
        {
            Damage = damage;
            GovernedSkill = governedSkill;
            ActionPointsRequired = actionPointsRequired;
        }
    }

}
