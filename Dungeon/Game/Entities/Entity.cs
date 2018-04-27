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
    }
}
