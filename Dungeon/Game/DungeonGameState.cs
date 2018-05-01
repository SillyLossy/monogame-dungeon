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
        private static readonly IReadOnlyList<FloorSettings> PredefinedSettings = new List<FloorSettings>
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
        private int currentFloor = 0;

        [JsonIgnore]
        public DungeonFloor CurrentFloor => floors[currentFloor];

        [JsonIgnore]
        public Player Player => CurrentFloor.Entities.OfType<Player>().FirstOrDefault();

        [JsonProperty]
        private Dictionary<int, HashSet<Point>> seenPoints = new Dictionary<int, HashSet<Point>>();
        
        private static int NewSeed => (int) DateTime.UtcNow.Ticks;

        public DungeonGameState()
        {
        }

        public DungeonGameState(bool generateFloors = false)
        {
            if (generateFloors)
            {
                for (int i = 0; i < PredefinedSettings.Count; i++)
                {
                    GenerateFloor(i, PredefinedSettings[i]);
                }
                CurrentFloor.PlacePlayer(new Player(CurrentFloor.RandomEntranceNeighbor));
                Player.SeenPoints = seenPoints[currentFloor];
                Player.SteppedOnLadder += ChangeFloor;
            }
        }

        private void GenerateFloor(int index, FloorSettings settings)
        {
            floors.Add(DungeonGenerator.GenerateFloor(NewSeed, settings));
            seenPoints.Add(index, new HashSet<Point>());
        }

        // Advances the game state forward in time
        public void Step()
        {
            try
            {
                foreach (var entity in CurrentFloor.Entities.OfType<MovableEntity>().Where(e => e.IsMoving))
                {
                    entity.Step(CurrentFloor);
                }
            }
            catch (InvalidOperationException)
            {
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
            if (point.X < CurrentFloor.Settings.Width && point.Y < CurrentFloor.Settings.Height && point.Y >= 0 && point.X >= 0)
            {
                if (Player.SeenPoints.Contains(point))
                {
                    Player.MoveTo(CurrentFloor, point);
                }
            }
        }

        private void ChangeFloor(object sender, DungeonTile ladder)
        {
            // only player can change floors (sanity check)
            if (sender is Player player)
            {
                player.Path.Clear();
                player.IsMoving = false;
                if (ladder == DungeonTile.LadderUp)
                {
                    if (currentFloor - 1 < 0)
                    {
                        // don't let him escape : )
                        return;
                    }

                    CurrentFloor.RemovePlayer(player);
                    currentFloor--;
                    player.Position = CurrentFloor.RandomExitNeighbor;
                    player.SeenPoints = seenPoints[currentFloor];
                    CurrentFloor.PlacePlayer(player);
                }
                else if (ladder == DungeonTile.LadderDown)
                {
                    if (currentFloor + 1 >= floors.Count)
                    {
                        GenerateFloor(currentFloor + 1, new FloorSettings());
                    }

                    CurrentFloor.RemovePlayer(player);
                    currentFloor++;
                    player.Position = CurrentFloor.RandomEntranceNeighbor;
                    player.SeenPoints = seenPoints[currentFloor];
                    CurrentFloor.PlacePlayer(player);
                }
            }
        }
    }
}
