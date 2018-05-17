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
        public Floor CurrentFloor
        {
            get
            {
                return World.Levels.Find(x => x.Location == PlayerLocation).Floors[PlayerLocation.Depth];
            }
        }

        private World.World World { get; } = new World.World(NewSeed);

        public Location PlayerLocation { get; set; }

        public Player Player { get; private set; }

        public TextGameLog Log { get; } = new TextGameLog();

        private SimplePriorityQueue<Character, int> CurrentQueue { get; set; } = new SimplePriorityQueue<Character, int>();

        private SimplePriorityQueue<Character, int> NextQueue { get; set; } = new SimplePriorityQueue<Character, int>();

        private Dictionary<Location, HashSet<Point>> SeenPoints { get; } = new Dictionary<Location, HashSet<Point>>();

        private static int NewSeed => (int)DateTime.UtcNow.Ticks;

        public event EventHandler<Point> PlayerWarped; 

        public void NewGame()
        {
            PlayerLocation = World.InitialLocation;

            SeenPoints[PlayerLocation] = new HashSet<Point>();
            var initialPoint = World.Levels.Find(x => x.Location == PlayerLocation).Floors[PlayerLocation.Depth].EntrancePoint;
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
                Sprites.Player)
            {
                SeenPoints = SeenPoints[PlayerLocation]
            };
            WarpPlayer(new Warp(PlayerLocation, initialPoint));
        }

        private ulong Ticks { get; set; }

        private const ulong RegenRate = 100;

        // Advances the game state forward in time
        public bool Update(Func<ActionResult> inputAction)
        {
            if (inputAction == null && !Player.HasNextStep)
            {
                return false;
            }

            var characters = CurrentFloor.Entities.OfType<Character>().ToArray();

            Ticks++;
            if (Ticks % RegenRate == 0)
            {
                foreach (var character in characters)
                {
                    character.Regenerate();
                }
            }

            Player.PendingAction = inputAction;
            Player.Update(this);
            NextQueue.Enqueue(Player, -Player.Sequence);

            if (CurrentQueue.Count == 0)
            {
                foreach (var character in CurrentFloor.Entities.OfType<Character>())
                {
                    CurrentQueue.Enqueue(character, -character.Sequence);
                }
            }

            while (CurrentQueue.Count != 0)
            {
                var current = CurrentQueue.Dequeue();

                if (current is Player)
                {
                    return true;
                }

                var actionResult = current.Update(this);
                Log.LogActionResult(actionResult);
                NextQueue.Enqueue(current, -current.Sequence);
            }

            var tempQueue = CurrentQueue;
            CurrentQueue = NextQueue;
            NextQueue = tempQueue;
            NextQueue.Clear();

            return true;
        }

        public ActionResult MovePlayer(Direction direction)
        {
            return Player.MoveTo(CurrentFloor, direction);
        }

        public ActionResult MovePlayer(Point point)
        {
            if (point.X < CurrentFloor.Settings.Width && point.Y < CurrentFloor.Settings.Height && point.Y >= 0 && point.X >= 0)
            {
                if (Player.SeenPoints.Contains(point))
                {
                    if (point == CurrentFloor.Entities.Reverse[Player])
                    {
                        if (CurrentFloor.Tiles[point] is WarpTile warp)
                        {
                            return WarpPlayer(warp.Warp);
                        }
                    }
                    else
                    {
                        return Player.MoveTo(CurrentFloor, point);
                    }
                }
            }
            return ActionResult.Empty;
        }

        public ActionResult Ascend()
        {
            if (CurrentFloor.Tiles[CurrentFloor.Entities.Reverse[Player]] is WarpTile warp)
            {
                if (warp.Warp.Location.Id == PlayerLocation.Id)
                {
                    if (warp.Warp.Location.Depth < PlayerLocation.Depth)
                    {
                        return WarpPlayer(warp.Warp);
                    }
                }
            }
            return new SimpleResult("You can't go up here", LineType.Info);
        }

        private ActionResult WarpPlayer(Warp warp)
        {
            CurrentFloor.Entities.TryRemove(Player);
            PlayerLocation = warp.Location;
            CurrentFloor.Entities.Add(warp.Point, Player);

            HashSet<Point> seenPointsInWarpLocation;

            if (SeenPoints.ContainsKey(PlayerLocation))
            {
                seenPointsInWarpLocation = SeenPoints[PlayerLocation];
            }
            else
            {
                seenPointsInWarpLocation = new HashSet<Point>();
                SeenPoints[PlayerLocation] = seenPointsInWarpLocation;
            }

            Player.SeenPoints = seenPointsInWarpLocation;
            Player.VisiblePoints = Player.GetVisiblePoints(CurrentFloor, PlayerLocation);
            PlayerWarped?.Invoke(this, CurrentFloor.Entities.Reverse[Player]);
            switch (warp.Location.Type)
            {
                case LocationType.Island:
                    return new SimpleResult("You go to the island", LineType.Info);
                default:
                    return new SimpleResult(string.Format("You go to the level {0}", warp.Location.Depth + 1));
            }
        }

        public ActionResult Descend()
        {
            if (CurrentFloor.Tiles[CurrentFloor.Entities.Reverse[Player]] is WarpTile warp)
            {
                if (warp.Warp.Location.Id == PlayerLocation.Id)
                {
                    if (warp.Warp.Location.Depth > PlayerLocation.Depth)
                    {
                        return WarpPlayer(warp.Warp);
                    }
                }
            }
            return new SimpleResult("You can't go down here", LineType.Info);
        }
    }
}
