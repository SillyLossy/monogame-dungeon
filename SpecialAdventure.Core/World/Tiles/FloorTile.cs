namespace SpecialAdventure.Core.World.Tiles
{
    public class FloorTile : Tile
    {
        public override bool IsPassable => true;
        public override bool IsTransparent => true;

        public FloorTile(int spriteId) : base(spriteId)
        {
        }
    }
}