using System.Collections.Generic;

namespace SpecialAdventure.Core.World.Levels
{
    public class Island : AbstractLevel
    {
        public Island(IReadOnlyList<Floor> floors) : base(floors)
        {
        }
        
        public override LocationType Type => LocationType.Island;
    }
}
