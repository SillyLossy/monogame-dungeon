using System.Collections.Generic;

namespace Dungeon.Game.World.Levels
{
    public class Island : AbstractLevel
    {
        public Island(IReadOnlyList<Floor> floors) : base(floors)
        {
        }
        
        public override LocationType Type => LocationType.Island;
    }
}
