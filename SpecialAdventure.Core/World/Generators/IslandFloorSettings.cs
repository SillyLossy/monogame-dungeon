using System;
using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.World.Generators
{
    public class IslandFloorSettings : AbstractFloorSettings
    {
        public int TerrainVariability { get; private set; }

        public static IslandFloorSettings GetSettings(Random random)
        {
            const int minVariability = 400;
            const int maxVariability = 600;

            var terrainVariabilityRange = new IntRange(minVariability, maxVariability)
            {
                Random = random
            };

            return new IslandFloorSettings
            {
                Height = 2048,
                Width = 2048,
                TerrainVariability = terrainVariabilityRange.Value
            };
        }
    }
}
