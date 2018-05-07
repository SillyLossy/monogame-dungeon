using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeon.Game.Entities.Items
{
    public class Inventory
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public Dictionary<EquipSlot, Item> Equipped { get; set; } = new Dictionary<EquipSlot, Item>();
        public int CarryWeight => (int)Math.Floor(Items.Sum(x => x.Weight));
    }
}
