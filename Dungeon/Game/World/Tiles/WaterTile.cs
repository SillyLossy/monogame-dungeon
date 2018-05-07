namespace Dungeon.Game.World.Tiles
{
    public class WaterTile : Tile
    {
        public override bool IsPassable => false;
        public override bool IsTransparent => true;

        public WaterTile(int spriteId) : base(spriteId)
        {
        }
    }
}