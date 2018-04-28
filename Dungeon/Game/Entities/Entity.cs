using System;
using System.Collections.Generic;
using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public class Entity
    {
        public bool IsMoving { get; set; }
        public Point Position { get; set; }
        public Stack<Point> Path { get; set; }
        private readonly DungeonFloor parent;

        public Entity(DungeonFloor parent, Point initialPosition)
        {
            this.parent = parent;
            Position = initialPosition;
            Path = new Stack<Point>();
        }

        public void MoveTo(Point target)
        {
            if (Position == target)
            {
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

        public void Step()
        {
            if (!IsMoving || Path.Count == 0)
            {
                return;
            }

            Position = Path.Pop();

            if (Path.Count == 0)
            {
                IsMoving = false;
            }
        }

        public void MoveTo(Direction direction)
        {
            var oldPos = Position;
            Position = NewPosition(direction, oldPos);
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
