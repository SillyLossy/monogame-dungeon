using Dungeon.Game.Common;

namespace Dungeon.Game.World.Generators
{
    public abstract class AbstractFloorSettings
    {
        public Point InitialPoint { get; set; }
        public int Depth { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public IntRange Monsters { get; protected set; }
    }
}
