﻿namespace Dungeon.Game.Entities
{
    public class Armor : Item
    {
        public Armor(string name, double weight, int basePrice, int armorClass, int damageThreshold, double damageResistance, string textureKey) : base(name, weight, basePrice, textureKey)
        {
            ArmorClass = armorClass;
            DamageThreshold = damageThreshold;
            DamageResistance = damageResistance;
        }

        public int ArmorClass { get; set; }
        public int DamageThreshold { get; set; }
        public double DamageResistance { get; set; }

        public static readonly Armor NoArmor = new Armor("", 0, 0, 0, 0, 0, Game.TextureKey.Empty);
    }
}
