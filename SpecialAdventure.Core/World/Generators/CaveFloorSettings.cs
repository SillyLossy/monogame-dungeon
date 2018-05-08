using System;
using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.World.Generators
{
    public class CaveFloorSettings : AbstractFloorSettings
    {
        public double ChanceToStartAlive { get; private set; }
        public int DeathLimit { get; private set; }
        public int BirthLimit { get; private set; }
        public int NumberOfSteps { get; private set; }
        
        private static readonly IntRange DeathLimitRange = new IntRange(2, 3); 
        private static readonly IntRange BirthLimitRange = new IntRange(3, 4);
        private static readonly IntRange NumberOfStepsRange = new IntRange(2, 4);
        private static readonly DoubleRange ChanceToStartAliveRange = new DoubleRange(0.3, 0.4);

        public static CaveFloorSettings GetSettings(int depth, Random random)
        {
            const int minSize = 30;
            const int maxSize = 50;
            const int sizeIncrement = 5;
            const int minMonsters = 3;
            const int maxMonsters = 5;

            const int monstersIncrement = 3;

            DeathLimitRange.Random = random;
            BirthLimitRange.Random = random;
            NumberOfStepsRange.Random = random;
            ChanceToStartAliveRange.Random = random;

            var widthRange = new IntRange(minSize + (depth * sizeIncrement),
                                          maxSize + (depth * sizeIncrement))
            {
                Nearest = sizeIncrement,
                Random = random
            };

            var heightRange = new IntRange(minSize + (depth * sizeIncrement),
                                           maxSize + (depth * sizeIncrement))
            {
                Nearest = sizeIncrement,
                Random = random
            };

            var monstersRange = new IntRange(minMonsters + (depth * monstersIncrement),
                                             maxMonsters + (depth * monstersIncrement))
            {
                Random = random
            };

            return new CaveFloorSettings
            {
                BirthLimit = BirthLimitRange.Value,
                DeathLimit = DeathLimitRange.Value,
                ChanceToStartAlive = ChanceToStartAliveRange.Value,
                NumberOfSteps = NumberOfStepsRange.Value,
                Width = widthRange.Value,
                Height = heightRange.Value,
                Depth = depth,
                Monsters = monstersRange
            };
        }
    }
}
