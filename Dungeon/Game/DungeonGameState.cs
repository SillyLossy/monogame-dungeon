using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Entities;
using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Dungeon.Game
{
    public class DungeonGameState
    {
        private static readonly IEnumerable<FloorSettings> PredefinedSettings = new List<FloorSettings>
        {
            new FloorSettings
            {
                Width = 32,
                Height = 32,
                MaxRooms = 5,
                MaxRoomXy = 10,
                MinRoomXy = 5,
                RandomConnections = 2,
                RandomSpurs = 2,
                RoomsOverlap = false
            }
        }.AsReadOnly();

        [JsonProperty]
        private readonly List<DungeonFloor> floors = new List<DungeonFloor>();

        [JsonProperty]
        private readonly int currentFloor = 0;

        [JsonIgnore]
        public DungeonFloor CurrentFloor => floors[currentFloor];

        public MovableEntity Player { get; set; }

        public DungeonGameState()
        {

        }

        public DungeonGameState(bool generateFloors = false)
        {
            if (generateFloors)
            {
                foreach (FloorSettings settings in PredefinedSettings)
                {
                    DungeonFloor floor = DungeonGenerator.GenerateFloor((int)DateTime.UtcNow.Ticks, settings);
                    floors.Add(floor);
                }
                Player = CurrentFloor.AddEntity();
            }
        }

        // Advances the game state forward in time
        public void Step()
        {
            foreach (var entity in CurrentFloor.Entities.OfType<MovableEntity>().Where(e => e.IsMoving))
            {
                entity.Step(CurrentFloor);
            }
        }
        
        public void MovePlayer(Direction direction)
        {
            if (CurrentFloor.CanEntityMove(Player, direction))
            {
                Player.MoveTo(CurrentFloor, direction);
            }

        }

        public void MovePlayer(Point point)
        {
            Player.MoveTo(CurrentFloor, point);
        }
    }
}
