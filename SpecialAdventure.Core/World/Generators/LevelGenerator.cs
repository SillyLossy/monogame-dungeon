using System;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.World.Levels;

namespace SpecialAdventure.Core.World.Generators
{
    public abstract class LevelGenerator
    {
        protected readonly Random Random;
        protected readonly IntRange DepthRange;
        private const int NearestDepth = 5;

        protected LevelGenerator(int seed, int minDepth, int maxDepth)
        {
            Random = new Random(seed);
            DepthRange = new IntRange(minDepth, maxDepth) { Random = Random, Nearest = NearestDepth };
        }

        public abstract AbstractLevel Generate();
    }
}
