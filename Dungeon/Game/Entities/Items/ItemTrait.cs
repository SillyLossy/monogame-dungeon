using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Common;
using Dungeon.Game.Entities.Common;

namespace Dungeon.Game.Entities.Items
{
    public class ItemTrait
    {
        public string Name { get; set; }
        public Rarity Rarity { get; set; }
        public Action<Item> ApplyTrait { get; set; }

        private static Rarity RollRarity()
        {
            double roll = DungeonGame.Random.NextDouble();

            if (roll < 0.5)
            {
                return Rarity.Common;
            }

            if (roll < 0.7)
            {
                return Rarity.Uncommon;
            }

            if (roll < 0.85)
            {
                return Rarity.Rare;
            }

            if (roll < 0.95)
            {
                return Rarity.Mythical;
            }

            if (roll < 0.95)
            {
                return Rarity.Legendary;
            }
            
            return Rarity.Divine;
        }

        public static ItemTrait GetRandomTrait(IReadOnlyList<ItemTrait> traits)
        {
            ItemTrait trait = null;
            do
            {
                var rarity = RollRarity();
                var acceptableTraits = traits.Where(t => t.Rarity == rarity).ToList();

                if (acceptableTraits.Count == 0)
                {
                    continue;
                }

                acceptableTraits.Shuffle();
                trait = acceptableTraits[0];
            } while (trait == null);

            return trait;
        }

    }
}
