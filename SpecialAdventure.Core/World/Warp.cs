using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.World
{
    public class Warp
    {
        public Location Location { get; }
        public Point Point { get; }

        public Warp(Location location, Point point)
        {
            Location = location;
            Point = point;
        }
    }
}
