using System;
using System.Collections.Generic;

namespace Dungeon.Game.Entities
{
    public class WeaponTrait : ItemTrait
    {
        public static readonly IReadOnlyList<ItemTrait> Traits = new[]
        {
            new ItemTrait
            {
                Name = "Broken",
                Rarity = Rarity.Common,
                ApplyTrait = (item) =>
                {
                    CheckItemType(item);
                    var weapon = (Weapon) item;
                    weapon.BasePrice /= 2;
                    weapon.Damage.Min /= 2;
                    weapon.Damage.Max /= 2;
                }
            },
            new ItemTrait
            {
                Name = "Rusty",
                Rarity = Rarity.Common,
                ApplyTrait = (item) =>
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

        public static WeaponTrait GetRandom()
        {
            Rarity rarity;
            int roll = DungeonGame.Random.Next(0, 990);

            if (roll < 500)
            {
                rarity = Rarity.Common;
            }
            else if (roll < 750)
            {
                rarity = Rarity.Uncommon;
            }

            else if (roll < 875)
            {
                rarity = Rarity.Rare;
            }

            else if (roll < 937)
            {
                rarity = Rarity.Mythical;
            }

            else if (roll < 968)
            {
                rarity = Rarity.Legendary;
            }
            else if (roll < 983)
            {
                rarity = Rarity.Divine;
            }

            return null;
        }

    }
}