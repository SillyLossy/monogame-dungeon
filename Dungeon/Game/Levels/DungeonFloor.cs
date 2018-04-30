using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Dungeon.Game.Levels
{
    public class DungeonFloor
    {
        public DungeonTile[,] Tiles { get; }

        [JsonIgnore]
        public Point RandomFreePoint
        {
            get
            {
                Point point = Point.Zero;
                for (int i = 0; i < Settings.Width * Settings.Height; i++)
                {
                    int x = random.Next(Settings.Width);
                    int y = random.Next(Settings.Height);
                    var randomPoint = new Point(x, y);

                    if (Tiles[x, y] != DungeonTile.Floor || entities.Find(e => e.Position == randomPoint) != null)
                    {
                        continue;
                    }

                    point = randomPoint;
                    break;
                }
                return point;
            }
        }

        public IEnumerable<Entity> Entities => entities;
        public FloorSettings Settings { get; }
        public List<Door> Doors { get; set; }

        public Point InitialPlayerPosition
        {
            get
            {
                Point? entrancePoint = null;
                for (int x = 0; x < Settings.Width; x++)
                {
                    for (int y = 0; y < Settings.Height; y++)
                    {
                        if (Tiles[x, y] == DungeonTile.LadderUp)
                        {
                            entrancePoint = new Point(x, y);
                        }
                    }
                }

                if (entrancePoint == null)
                {
                    throw new Exception("No entrances on level!");
                }

                var entranceNeighbors = GetNeighbors(entrancePoint.Value).ToArray();
                return entranceNeighbors[random.Next(entranceNeighbors.Length)];
            }
        }

        private static readonly Random random = new Random();
        private readonly List<Entity> entities;


        public DungeonFloor(DungeonTile[,] tiles, FloorSettings settings)
        {
            Tiles = tiles;
            Settings = settings;
            entities = new List<Entity>();
        }
        
        public IEnumerable<Point> GetNeighbors(Point current, bool ignoreEntities = false)
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
                        var point = new Point(x, y);

                        if (ignoreEntities == false)
                        {
                            var closedDoor = Doors.FirstOrDefault(d => !d.IsOpen && d.Position == point);
                            var entity = Entities.FirstOrDefault(e => e.Position == point);
                            if (closedDoor == null && entity == null)
                            {
                                yield return point;
                            }
                        }
                        else
                        {
                            yield return point;
                        }
                    }
                }
            }
        }

        public void PlacePlayer(Point? initialPosition = null)
        {
            Point position = initialPosition ?? InitialPlayerPosition;
            var player = new Player(position);
            entities.Add(player);
        }

        public bool CanEntityMove(MovableEntity entity, Direction direction)
        {
            var newPos = MovableEntity.NewPosition(direction, entity.Position);
            return Tiles[newPos.X, newPos.Y] == DungeonTile.Floor;
        }
    }
}
