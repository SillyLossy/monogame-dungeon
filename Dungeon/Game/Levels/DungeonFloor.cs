using System;
using System.Collections.Generic;
using Dungeon.Game.Entities;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Levels
{
    [Serializable]
    public class DungeonFloor
    {
        public DungeonTile[,] Tiles { get; }

        public Point RandomFreePoint
        {
            get
            {
                Point point;
                while (true)
                {
                    int x = random.Next(Settings.Width);
                    int y = random.Next(Settings.Height);
                    if (Tiles[x, y] == DungeonTile.Floor)
                    {
                        point = new Point(x, y);
                        break;
                    }
                }
                return point;
            }
        }

        public IEnumerable<Entity> Entities => entities;
        public FloorSettings Settings { get; }

        private static readonly Random random = new Random();
        private readonly List<Entity> entities;


        public DungeonFloor(DungeonTile[,] tiles, FloorSettings settings)
        {
            Tiles = tiles;
            Settings = settings;
            entities = new List<Entity>();
        }
        
        public IEnumerable<Point> GetNeighbors(Point current)
        {
            // -1 0 1
            for (int i = -1; i < 2; i++)
            {
                // -1 0 1
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    int x = current.X + i, y = current.Y + j;
                    if (Tiles[x, y] == DungeonTile.Floor)
                    {
                        yield return (new Point(x, y));
                    }
                }
            }
        }

        public Entity AddEntity(Point? initialPosition = null)
        {
            Point position = initialPosition ?? RandomFreePoint;
            var entity = new Entity(this, position);
            entities.Add(entity);
            return entity;
        }

        public bool CanEntityMove(Entity entity, Direction direction)
        {
            var newPos = Entity.NewPosition(direction, entity.Position);
            return Tiles[newPos.X, newPos.Y] == DungeonTile.Floor;
        }
    }
}
