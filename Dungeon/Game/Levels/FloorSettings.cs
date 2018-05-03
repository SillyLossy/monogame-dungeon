using System;

namespace Dungeon.Game.Levels
{
    public class FloorSettings
    {
        public int Width { get; set; } = 64;
        public int Height { get; set; } = 64;
        public int MaxRooms { get; set; } = 15;
        public int MinRoomXy { get; set; } = 5;
        public int MaxRoomXy { get; set; } = 10;
        public int MinMonsters { get; set; } = 5;
        public int MaxMonsters { get; set; } = 20;
        public bool RoomsOverlap { get; set; } = false;
        public int RandomConnections { get; set; } = 1;
        public int RandomSpurs { get; set; } = 3;
        public int FloorIndex { get; }

        public FloorSettings(int index)
        {
            FloorIndex = index;
        }
    }
}
