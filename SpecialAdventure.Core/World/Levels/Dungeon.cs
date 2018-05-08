using System.Collections.Generic;

namespace SpecialAdventure.Core.World.Levels
{
    public class Dungeon : AbstractLevel
    {
        public override LocationType Type => LocationType.Dungeon;

        public Dungeon(IReadOnlyList<Floor> floors) : base(floors)
        {
        }
    }
}
