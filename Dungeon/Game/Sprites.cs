using System.Diagnostics.PerformanceData;
using Dungeon.Game.World.Tiles;

namespace Dungeon.Game
{
    public static class Sprites
    {
        private const string TexturesPath = "Textures";

        public const int Wall = 0;

        public const int Floor = 1;

        public const int Stone = 2;

        public const int LadderUp = 3;

        public const int LadderDown = 4;

        public const int ValleyGrass = 5;

        public const int Water = 6;

        public const int ShallowWater = 7;

        public const int Sand = 8;

        public const int Mountain = 9;

        public const int Snow = 10;

        public const int DoorClosed = 11;

        public const int DoorBroken = 12;

        public const int DoorOpen = 13;

        public const int Goblin = 14;

        public const int Player = 15;

        public const int YellowScorpion = 16;

        public const int GreenTortoise = 17;

        public const int Reserved = 18;

        public const int ForestGrass = 19;

        public static string FromTile(Tile tile)
        {
            return $"{TexturesPath}/{tile}";
        }
    }
}