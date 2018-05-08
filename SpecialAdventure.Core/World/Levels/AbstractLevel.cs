using System.Collections.Generic;

namespace SpecialAdventure.Core.World.Levels
{
    public abstract class AbstractLevel
    {
        public abstract LocationType Type { get; }

        public int MaxDepth { get; set; }

        public IReadOnlyList<Floor> Floors { get; }

        protected AbstractLevel(IReadOnlyList<Floor> floors)
        {
            Floors = floors;
        }
    }
}
