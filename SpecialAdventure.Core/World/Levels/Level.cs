using System.Collections.Generic;

namespace SpecialAdventure.Core.World.Levels
{
    public class Level
    {
        public Location Location { get; set; }

        public IReadOnlyList<Floor> Floors { get; }

        public Level(Location location, IReadOnlyList<Floor> floors)
        {
            Location = location;
            Floors = floors;
        }
    }
}
