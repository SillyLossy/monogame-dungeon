namespace Dungeon.Game.Entities
{
    public class Item : GameObject
    {
        public Item(string name, double weight, int basePrice, string textureKey) : base(textureKey)
        {
            Weight = weight;
            BasePrice = basePrice;
        }

        public double Weight { get; set; }
        public int BasePrice { get; set; }
        public string Name { get; set; }
    }
}