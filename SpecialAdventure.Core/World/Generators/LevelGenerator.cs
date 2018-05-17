using System;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.World.Levels;

namespace SpecialAdventure.Core.World.Generators
{
    public abstract class LevelGenerator
    {
        protected readonly Random Random;
        protected readonly IntRange DepthRange;
        protected readonly World ParentWorld;

        protected LevelGenerator(World world, int seed, int minDepth, int maxDepth)
        {
            ParentWorld = world;
            Random = new Random(seed);
            DepthRange = new IntRange(minDepth, maxDepth) { Random = Random };
        }

        public abstract Level Generate(Warp returnWarp);
    }
}
