using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using SpecialAdventure.Core.Entities.Characters;
using SpecialAdventure.Core.Entities.Common;
using SpecialAdventure.Core.Log;
using SpecialAdventure.Core.World;
using SpecialAdventure.Core.World.Tiles;

namespace SpecialAdventure.Core.Common
{
    public class GameState
    {
        private List<Floor> floors = new List<Floor>();
        
        private int currentFloor = 0;
        
        public Floor CurrentFloor
        {
            get
            {
                switch (PlayerLocation.Type)
                {
                    case LocationType.Dungeon:
                        return World.Dungeons[PlayerLocation.Id].Floors[PlayerLocation.Depth];
                    case LocationType.Cave:
                        return World.Caves[PlayerLocation.Id].Floors[PlayerLocation.Depth];
                    case LocationType.Island:
                        return World.Islands[PlayerLocation.Id].Floors[0];
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public World.World World { get; } = new World.World(NewSeed);

        public Location PlayerLocation = new Location(LocationType.Island, Guid.Empty, 0);
        
        public Character Player { get; set; }

        public TextGameLog Log { get; } = new TextGameLog();

        private SimplePriorityQueue<Character, int> CurrentQueue { get; set; } = new SimplePriorityQueue<Character, int>();

        private SimplePriorityQueue<Character, int> NextQueue { get; set; } = new SimplePriorityQueue<Character, int>();
        
        private Dictionary<Location, HashSet<Point>> seenPoints = new Dictionary<Location, HashSet<Point>>();

        private static int NewSeed => (int)DateTime.UtcNow.Ticks;

        public GameState NewGame()
        {
            var initialLocationId = World.GenerateIsland();

            PlayerLocation = new Location(LocationType.Island, initialLocationId);

            seenPoints[PlayerLocation] = new HashSet<Point>();

            Player = new Player(
                    "Player",
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
                    World.Islands[initialLocationId].Floors[0].Settings.InitialPoint,
                    Sprites.Player);
            Player.SeenPoints = seenPoints[PlayerLocation];
            return this;
        }

        // Advances the game state forward in time
        public void Update(Action inputAction)
        {
            return;
            if (WaitingForInput)
            {
                return;
            }

            if (CurrentQueue.Count == 0)
            {
                foreach (var character in CurrentFloor.Characters.OrderByDescending(c => c.Sequence))
                {
                    CurrentQueue.Enqueue(character, -character.Sequence);
                }
            }

            while (CurrentQueue.Count != 0)
            {
                var current = CurrentQueue.Dequeue();
                if (current == Player)
                {
                    WaitingForInput = true;
                }

                var result = current.Update(this);
                Log.LogActionResult(result);
                NextQueue.Enqueue(current, -current.Sequence);
            }

            var tempQueue = CurrentQueue;
            CurrentQueue = NextQueue;
            NextQueue = tempQueue;
            NextQueue.Clear();
        }

        public bool WaitingForInput { get; set; }

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
                        if (CurrentFloor.Tiles[point] == Tile.LadderDown)
                        {
                            Descend();
                        }
                        else if (CurrentFloor.Tiles[point] == Tile.LadderUp)
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
            if (CurrentFloor.Tiles[Player.Position] == Tile.LadderUp)
            {
                if (currentFloor - 1 < 0)
                {
                    // don't let him escape : )
                    return;
                }

                ReplacePlayer(currentFloor - 1);
                CurrentFloor.GenerateMonsters(isFirstEnter: false);
            }
            else
            {
                Log.PushLine(new LogLine("You can't go up here", LineType.Info));
            }
        }

        public void Descend()
        {
            if (CurrentFloor.Tiles[Player.Position] == Tile.LadderDown)
            {
                ReplacePlayer(currentFloor + 1);
                CurrentFloor.GenerateMonsters(isFirstEnter: false);
            }
            else
            {
                Log.PushLine(new LogLine("You can't go down here", LineType.Info));
            }
        }

        private void ReplacePlayer(int newFloor)
        {
            Character player = CurrentFloor.RemovePlayer(Player);
            PlayerLocation = new Location(PlayerLocation.Type, PlayerLocation.Id, newFloor);
            currentFloor = newFloor;
            player.Position = CurrentFloor.RandomEntranceNeighbor;
            player.SeenPoints = seenPoints[PlayerLocation];
            CurrentFloor.PlacePlayer(player);
            CurrentQueue.Clear();
            NextQueue.Clear();
        }

        private Dictionary<Point, Tile> lastTiles;
        private Rectangle lastRectangle;

        public Dictionary<Point, Tile> GetTiles(Common.Rectangle rectangle)
        {
            var loc = PlayerLocation;

            switch (loc.Type)
            {
                case LocationType.Island:
                    if (rectangle == lastRectangle)
                    {
                        return lastTiles;
                    }
                    var tiles = new Dictionary<Point, Tile>();
                    int maxW = World.Islands[loc.Id].Floors[0].Settings.Width;
                    int maxH = World.Islands[loc.Id].Floors[0].Settings.Height;
                    var islandTerrain = World.Islands[loc.Id].Floors[0].Tiles;
                    for (int i = rectangle.X; i < rectangle.W + rectangle.X; i++)
                    {
                        for (int j = rectangle.Y; j < rectangle.H + rectangle.Y; j++)
                        {
                            if (i < 0 || j < 0 || i >= maxW || j >= maxH)
                            {
                                continue;
                            }
                            var point = new Point(i, j);
                            tiles[point] = islandTerrain[point];
                        }
                    }
                    lastRectangle = rectangle;
                    lastTiles = tiles;
                    return tiles;
                case LocationType.Dungeon:
                    return World.Dungeons[loc.Id].Floors[loc.Depth].Tiles;
                case LocationType.Cave:
                    return World.Caves[loc.Id].Floors[loc.Depth].Tiles;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
