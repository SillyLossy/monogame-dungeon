namespace SpecialAdventure.Core.World.Tiles
{
    public class WarpTile : FloorTile
    {
        public Warp Warp { get; set; }

        public WarpTile(Warp warp, int spriteId) : base(spriteId)
        {
            Warp = warp;
        }
    }
}