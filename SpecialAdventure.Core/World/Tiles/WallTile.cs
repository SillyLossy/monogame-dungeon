namespace SpecialAdventure.Core.World.Tiles
{
    public class WallTile : Tile
    {
        public override bool IsPassable => false;
        public override bool IsTransparent => false;

        public WallTile(int spriteId) : base(spriteId)
        {
        }
    }
}