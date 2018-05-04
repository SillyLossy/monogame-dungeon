using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Common;
using Dungeon.Game.Entities;
using Dungeon.Game.Levels;
using Newtonsoft.Json;

namespace Dungeon.Game
{
    public class DungeonGameState
    {
        private static readonly IReadOnlyList<FloorSettings> PredefinedSettings = new List<FloorSettings>
        {
            new FloorSettings(1)
            {
                Width = 64,
                Height = 64,
                MaxRooms = 10,
                MaxRoomXy = 15,
                MinRoomXy = 7,
                RandomConnections = 4,
                RandomSpurs = 4,
                RoomsOverlap = false,
                MaxMonsters = 10,
                MinMonsters = 4
            }
        }.AsReadOnly();

        [JsonProperty]
        private List<DungeonFloor> floors = new List<DungeonFloor>();

        [JsonProperty]
        private int currentFloor = 0;

        [JsonIgnore]
        public DungeonFloor CurrentFloor => floors[currentFloor];

        [JsonIgnore]
        public Character Player => CurrentFloor.Characters.OfType<Player>().FirstOrDefault();

        [JsonProperty]
        private Dictionary<int, HashSet<Point>> seenPoints = new Dictionary<int, HashSet<Point>>();

        private static int NewSeed => (int)DateTime.UtcNow.Ticks;

        public DungeonGameState NewGame()
        {
            for (int i = 0; i < PredefinedSettings.Count; i++)
            {
                GenerateFloor(i, PredefinedSettings[i]);
            }
            CurrentFloor.PlacePlayer(
                new Player(
                    "Player",
                    TextureKey.Player,
                    new PrimaryAttributes
                    {
                        Agility = 5,
                        Charisma = 5,
                        Endurance = 5,
                        Intelligence = 5,
                        Luck = 5,
                        Perception = 5,
                        Strength = 5
                    },
                    CurrentFloor.RandomEntranceNeighbor));
            Player.SeenPoints = seenPoints[currentFloor];
            return this;
        }

        private void GenerateFloor(int index, FloorSettings settings)
        {
            floors.Add(DungeonGenerator.GenerateFloor(NewSeed, settings));
            seenPoints.Add(index, new HashSet<Point>());
        }

        // Advances the game state forward in time
        public void Update(Action inputAction)
        {
            // Only do update if we have a pending action or the player is already moving.
            if (inputAction == null && !Player.HasNextStep)
            {
                return;
            }

            inputAction?.Invoke();

            foreach (var character in CurrentFloor.Characters.OrderByDescending(c => c.Sequence))
            {
                character.Update(this);
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
                    if (point == Player.Position)
                    {
                        if (CurrentFloor.Tiles[point.X, point.Y] == DungeonTile.LadderDown)
                        {
                            Descend();
                        }
                        else if (CurrentFloor.Tiles[point.X, point.Y] == DungeonTile.LadderUp)
                        {
                            Ascend();
                        }
                    }
                    else
                    {
                        Player.MoveTo(CurrentFloor, point);
                    }
                }
            }
        }

        public void Ascend()
        {
            if (CurrentFloor.Tiles[Player.Position.X, Player.Position.Y] == DungeonTile.LadderUp)
            {
                if (currentFloor - 1 < 0)
                {
                    // don't let him escape : )
                    return;
                }

                ReplacePlayer(currentFloor - 1);
                CurrentFloor.GenerateMonsters(isFirstEnter: false);
            }
        }

        public void Descend()
        {
            if (CurrentFloor.Tiles[Player.Position.X, Player.Position.Y] == DungeonTile.LadderDown)
            {
                if (currentFloor + 1 >= floors.Count)
                {
                    GenerateFloor(currentFloor + 1, new FloorSettings(currentFloor + 2));
                }

                ReplacePlayer(currentFloor + 1);
                CurrentFloor.GenerateMonsters(isFirstEnter: false);
            }
        }

        private void ReplacePlayer(int newFloor)
        {
            Character player = CurrentFloor.RemovePlayer(Player);
            currentFloor = newFloor;
            player.Position = CurrentFloor.RandomEntranceNeighbor;
            player.SeenPoints = seenPoints[currentFloor];
            CurrentFloor.PlacePlayer(player);
        }
    }
}
