using System;
using System.Collections.Generic;
using Dungeon.Game.Entities;
using Dungeon.Game.Levels;

namespace Dungeon.Game
{
    [Serializable]
    public class DungeonGameState
    {
        private static readonly IEnumerable<FloorSettings> PredefinedSettings = new List<FloorSettings>
        {
            new FloorSettings
            {
                Width = 64,
                Height = 64,
                MaxRooms = 10,
                MaxRoomXy = 20,
                MinRoomXy = 5,
                RandomConnections = 3,
                RandomSpurs = 3,
                RoomsOverlap = false
            }
        }.AsReadOnly();
        
        private readonly List<DungeonFloor> floors = new List<DungeonFloor>();
        private readonly int currentFloor = 0;

        public DungeonFloor CurrentFloor => floors[currentFloor];

        public Entity Player { get; set; }

        public DungeonGameState()
        {
            foreach (var settings in PredefinedSettings)
            {
                var floor = DungeonGenerator.GenerateFloor((int) DateTime.UtcNow.Ticks, settings);
                floors.Add(floor);
            }
            Player = CurrentFloor.AddEntity();
        }

        // Advances the game state forward in time
        public void Step()
        {
            
        }
    }
}
