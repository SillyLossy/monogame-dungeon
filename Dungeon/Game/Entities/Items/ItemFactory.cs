namespace Dungeon.Game.Entities.Items
{
    public class ItemFactory
    {
        public string Name { get; set; }
        public int BasePrice { get; set; }
        public double Weight { get; set; }
        public int SpriteId { get; set; }

        public virtual Item Construct()
        {
            return new Item(Name, Weight, BasePrice, SpriteId);
        }
    }
}