using System.Collections.Generic;

namespace Dungeon.Game.World.Levels
{
    public class Cave : AbstractLevel
    {
        public override LocationType Type => LocationType.Cave;

        public Cave(IReadOnlyList<Floor> floors) : base(floors)
        {
        }
    }
}
