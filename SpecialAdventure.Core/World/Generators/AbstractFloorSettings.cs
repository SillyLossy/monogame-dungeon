using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.World.Generators
{
    public abstract class AbstractFloorSettings
    {
        public int Depth { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public IntRange Monsters { get; protected set; }
    }
}
