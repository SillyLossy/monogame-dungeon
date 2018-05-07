using System;
using System.Collections.Generic;
using Dungeon.Game.Entities.Common;

namespace Dungeon.Game.Entities.Items
{
    public class WeaponTrait : ItemTrait
    {
        private static readonly IReadOnlyList<ItemTrait> Traits = new[]
        {
            new WeaponTrait
            {
                Name = "Broken",
                Rarity = Rarity.Common,
                ApplyTrait = item =>
                {
                    CheckItemType(item);
                    var weapon = (Weapon) item;
                    weapon.BasePrice /= 2;
                    weapon.Damage.Min /= 2;
                    weapon.Damage.Max /= 2;
                }
            },
            new WeaponTrait
            {
                Name = "Rusty",
                Rarity = Rarity.Common,
                ApplyTrait = item =>
                {
                    CheckItemType(item);
                    var weapon = (Weapon) item;
                    weapon.BasePrice = (int) (weapon.BasePrice / 1.5d);
                    weapon.Damage.Min /= (int) (weapon.BasePrice / 1.5d);
                    weapon.Damage.Max /= (int) (weapon.BasePrice / 1.5d);
                }
            }
        };

        private static void CheckItemType(Item item)
        {
            if (!(item is Weapon))
            {
                throw new ArgumentException("Cannot apply a weapon trait to not a weapon");
            }
        }

        public static ItemTrait GetRandom()
        {
            return GetRandomTrait(Traits);
        }
    }
}