namespace Dungeon.Game.Entities
{
    public class Armor : Item
    {
        public Armor(string name, double weight, int basePrice, int armorClass, int damageThreshold, double damageResistance, string textureKey) : base(name, weight, basePrice, textureKey)
        {
            ArmorClass = armorClass;
            DamageThreshold = damageThreshold;
            DamageResistance = damageResistance;
        }

        public int ArmorClass { get; private set; }
        public int DamageThreshold { get; private set; }
        public double DamageResistance { get; private set; }

        public static readonly Armor NoArmor = new Armor("", 0, 0, 0, 0, 0, Game.TextureKey.Empty);
    }
}
