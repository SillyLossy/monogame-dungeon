using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.FOV;
using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public abstract class MovableEntity : Entity
    {
        public bool IsMoving { get; set; }
        public LinkedList<Point> Path { get; private set; }

        protected MovableEntity(Point initialPosition) : base(initialPosition)
        {
            Path = new LinkedList<Point>();
        }

        public HashSet<Point> SeenPoints;

        public int SightRadius { get; set; } = 7;

        public HashSet<Point> GetVisiblePoints(DungeonFloor parent)
        {
            var algorithm = new PermissiveFov(parent.Settings.Width, parent.Settings.Height, parent.IsTransparent);
            var points = algorithm.Compute(Position.X, Position.Y, SightRadius);

            SeenPoints.UnionWith(points);
            return points;
        }

        private bool AddPoint(DungeonFloor parent, int x, int y, List<Door> nearDoors, HashSet<Point> points)
        {
            var point = new Point(x, y);
            var tile = parent.Tiles[x, y];

            // if we is the door
            if (nearDoors.Find(d => Position == point) != null)
            {
                points.Add(point);
                // break the loop so we can't see past the door
                return true;
            }

            // if we see the wall
            if (tile == DungeonTile.Wall)
            {
                points.Add(point);
                // break the loop so we can't see past the wall
                return true;
            }

            points.Add(point);
            return false;
        }


        // We can't move to non-floor tile directly, so move to nearest neighbor and then move to entity
        private void FindPathToEntity(DungeonFloor parent, Point target)
        {
            // find a shortest path to entity and move to it
            LinkedList<Point> shortestNeighborPath = null;
            foreach (Point neighbor in parent.GetNeighbors(target))
            {
                var path = PathFinder.AStar(parent, Position, neighbor);
                if (path == null)
                {
                    continue;
                }
                if (shortestNeighborPath == null || path.Count < shortestNeighborPath.Count)
                {
                    shortestNeighborPath = path;
                    shortestNeighborPath.AddLast(target);
                }
            }

            if (shortestNeighborPath == null)
            {
                // can't move, do nothing
                return;
            }

            Path = shortestNeighborPath;
            IsMoving = true;
        }

        public void MoveTo(DungeonFloor parent, Point target)
        {
            if (Position == target)
            {
                return;
            }

            var door = parent.Doors.FirstOrDefault(d => !d.IsOpen && d.Position == target);
            if (door != null)
            {
                // if door is neighbor, just open it
                if (parent.GetNeighbors(door.Position, ignoreEntities: true).Any(neighbor => neighbor == Position))
                {
                    door.Open();
                    return;
                }
                FindPathToEntity(parent, door.Position);
                return;
            }

            if (parent.Tiles[target.X, target.Y] == DungeonTile.LadderUp ||
                parent.Tiles[target.X, target.Y] == DungeonTile.LadderDown)
            {
                FindPathToEntity(parent, target);
            }

            var path = PathFinder.AStar(parent, Position, target);
            if (path == null)
            {
                // can't move, do nothing
                return;
            }

            Path = path;
            IsMoving = true;
        }

        public void Step(DungeonFloor parent)
        {
            if (!IsMoving || Path.Count == 0)
            {
                return;
            }

            Point newPos = Path.First.Value;
            Path.RemoveFirst();
            CheckForDoor(parent, newPos);

            if (!CheckForLadder(parent, newPos))
            {
                Position = newPos;
            }

            if (Path.Count == 0)
            {
                IsMoving = false;
            }
        }

        public void MoveTo(DungeonFloor parent, Direction direction)
        {
            Point oldPos = Position;
            Point newPos = NewPosition(direction, oldPos);

            CheckForDoor(parent, newPos);

            Position = newPos;
        }

        private bool CheckForLadder(DungeonFloor parent, Point newPos)
        {
            switch (parent.Tiles[newPos.X, newPos.Y])
            {
                case DungeonTile.LadderDown:
                    SteppedOnLadder?.Invoke(this, DungeonTile.LadderDown);
                    return true;
                case DungeonTile.LadderUp:
                    SteppedOnLadder?.Invoke(this, DungeonTile.LadderUp);
                    return true;
            }

            return false;
        }

        public event EventHandler<DungeonTile> SteppedOnLadder;

        private static void CheckForDoor(DungeonFloor parent, Point newPos)
        {
            Door closedDoor = parent.Doors.FirstOrDefault(d => !d.IsOpen && d.Position == newPos);
            closedDoor?.Open();
        }

        public static Point NewPosition(Direction direction, Point oldPos)
        {
            switch (direction)
            {
                case Direction.South:
                    return new Point(oldPos.X, oldPos.Y + 1);
                case Direction.North:
                    return new Point(oldPos.X, oldPos.Y - 1);
                case Direction.West:
                    return new Point(oldPos.X - 1, oldPos.Y);
                case Direction.East:
                    return new Point(oldPos.X + 1, oldPos.Y);
                case Direction.NorthEast:
                    return new Point(oldPos.X + 1, oldPos.Y - 1);
                case Direction.NorthWest:
                    return new Point(oldPos.X - 1, oldPos.Y - 1);
                case Direction.SouthEast:
                    return new Point(oldPos.X + 1, oldPos.Y + 1);
                case Direction.SouthWest:
                    return new Point(oldPos.X - 1, oldPos.Y + 1);
                case Direction.None:
                    return new Point(oldPos.X, oldPos.Y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
