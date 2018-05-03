namespace Dungeon.Game.Entities
{
    public class Weapon : Item
    {
        public Range Damage { get; set; }
        public Skill GovernedSkill { get; set; }
        public int ActionPointsRequired { get; set; }
        
        public static Weapon UnarmedWeapon(int damage)
        {
            const int UnarmedActionPointsRequirement = 3;
            return new Weapon("", 0, 0, new Range(damage), Skill.Unarmed, UnarmedActionPointsRequirement, Game.TextureKey.Empty);
        }

        public Weapon(string name, double weight, int basePrice, Range damage, Skill governedSkill, int actionPointsRequired, string textureKey) : base(name, weight, basePrice, textureKey)
        {
            Damage = damage;
            GovernedSkill = governedSkill;
            ActionPointsRequired = actionPointsRequired;
        }
    }

}
