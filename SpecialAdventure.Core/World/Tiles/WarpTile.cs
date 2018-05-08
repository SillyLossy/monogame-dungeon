using System;
using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.World.Tiles
{
    public class WarpTile : FloorTile
    {
        public int WarpDepth { get; set; }
        public Point WarpPoint { get; set; }
        public Guid LocationId { get; set; }

        public WarpTile(int spriteId) : base(spriteId)
        {
        }
    }
}