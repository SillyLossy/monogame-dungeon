using System;

namespace Dungeon.Game.Entities
{
    public class ItemTrait
    {
        public string Name { get; set; }
        public Rarity Rarity { get; set; }
        public Action<Item> ApplyTrait { get; set; }
    }
}
