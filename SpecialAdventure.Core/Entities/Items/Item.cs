﻿using SpecialAdventure.Core.Entities.Common;

namespace SpecialAdventure.Core.Entities.Items
{
    public class Item : GameObject
    {
        public Item(string name, double weight, int basePrice, int spriteId) : base(spriteId)
        {
            Weight = weight;
            BasePrice = basePrice;
        }

        public double Weight { get; set; }
        public int BasePrice { get; set; }
        public string Name { get; set; }
    }
}