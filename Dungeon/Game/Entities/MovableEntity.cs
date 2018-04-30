using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public abstract class MovableEntity : Entity
    {
        public bool IsMoving { get; private set; }
        public LinkedList<Point> Path { get; private set; }

        protected MovableEntity(Point initialPosition) : base(initialPosition)
        {
            Path = new LinkedList<Point>();
        }

        private void MoveToDoor(DungeonFloor parent, Door target)
        {
            // if door is neighbor, just open it
            if (parent.GetNeighbors(Position).Any(playerNeighbor => playerNeighbor == target.Position))
            {
                target.Open();
                return;
            }

            // else find a shortest path to door and move to it
            LinkedList<Point> shortestNeighborPath = null;
            foreach (Point doorNeighbor in parent.GetNeighbors(target.Position))
            {
                var path = PathFinder.AStar(parent, Position, doorNeighbor);
                if (path == null)
                {
                    continue;
                }
                if (shortestNeighborPath == null || path.Count < shortestNeighborPath.Count)
                {
                    shortestNeighborPath = path;
                    shortestNeighborPath.AddLast(target.Position);
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
                MoveToDoor(parent, door);
                return;
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
            Position = newPos;

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
