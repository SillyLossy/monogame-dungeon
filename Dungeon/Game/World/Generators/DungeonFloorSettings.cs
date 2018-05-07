using System;
using Dungeon.Game.Common;

namespace Dungeon.Game.World.Generators
{
    public class DungeonFloorSettings : AbstractFloorSettings
    {
        public int MaxRooms { get; private set; } = 15;
        public IntRange RoomSize { get; private set; }
        public bool RoomsOverlap { get; private set; } = false;
        public int RandomConnections { get; private set; } = 1;
        public int RandomSpurs { get; private set; } = 3;

        private static readonly IntRange RandomSpursRange = new IntRange(2, 6);
        private static readonly IntRange RandomConnectionsRange = new IntRange(0, 5);

        public static DungeonFloorSettings GetSettings(int depth, Random random)
        {
            bool roomsOverlap = random.NextDouble() > 0.5;

            const int minSize = 35;
            const int maxSize = 60;
            const int sizeIncrement = 5;

            const int minRooms = 7;
            const int maxRooms = 12;
            const int roomsIncrement = 2;

            const int minMonsters = 3;
            const int maxMonsters = 5;
            const int monstersIncrement = 4;

            RandomSpursRange.Random = random;
            RandomConnectionsRange.Random = random;

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

            var maxRoomsRange = new IntRange(minRooms + (depth * roomsIncrement),
                                             maxRooms + (depth * roomsIncrement))
            {
                Random = random
            };

            var monstersRange = new IntRange(minMonsters + (depth * monstersIncrement),
                                             maxMonsters + (depth * monstersIncrement))
            {
                Random = random
            };


            return new DungeonFloorSettings
            {
                Width = widthRange.Value,
                Height = heightRange.Value,
                MaxRooms = maxRoomsRange.Value,
                RandomSpurs = RandomSpursRange.Value,
                RandomConnections = RandomConnectionsRange.Value,
                Depth = depth,
                Monsters = monstersRange,
                RoomsOverlap = roomsOverlap
            };
        }
    }
}
