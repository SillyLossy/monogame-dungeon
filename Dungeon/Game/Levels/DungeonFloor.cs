using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Entities;
using Dungeon.Game.Common;
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
                Point point = null;
                for (int i = 0; i < Settings.Width * Settings.Height; i++)
                {
                    int x = DungeonGame.Random.Next(Settings.Width);
                    int y = DungeonGame.Random.Next(Settings.Height);
                    var randomPoint = new Point(x, y);

                    if (!IsTransparent(x, y))
                    {
                        continue;
                    }

                    point = randomPoint;
                    break;
                }

                if (point == null)
                {
                    throw new ArgumentException("No free points left");
                }

                return point;
            }
        }

        public IEnumerable<Character> Characters => characters;
        public FloorSettings Settings { get; }
        public List<Door> Doors { get; set; }

        public Point RandomEntranceNeighbor => RandomNeighborOfTile(DungeonTile.LadderUp);
        public Point RandomExitNeighbor => RandomNeighborOfTile(DungeonTile.LadderDown);

        private Point RandomNeighborOfTile(DungeonTile tile)
        {

            Point entrancePoint = null;
            for (int x = 0; x < Settings.Width; x++)
            {
                for (int y = 0; y < Settings.Height; y++)
                {
                    if (Tiles[x, y] == tile)
                    {
                        entrancePoint = new Point(x, y);
                    }
                }
            }

            if (entrancePoint == null)
            {
                throw new Exception("No entrances on level!");
            }

            var entranceNeighbors = GetNeighbors(entrancePoint).ToArray();
            return entranceNeighbors[DungeonGame.Random.Next(entranceNeighbors.Length)];
        }
        
        private readonly List<Character> characters;


        public DungeonFloor(DungeonTile[,] tiles, FloorSettings settings)
        {
            Tiles = tiles;
            Settings = settings;
            characters = new List<Character>();
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
                    if (IsPassableTile(Tiles[x, y]))
                    {
                        var point = new Point(x, y);

                        if (ignoreEntities == false)
                        {
                            var closedDoor = Doors.FirstOrDefault(d => !d.IsOpen && d.Position == point);
                            var entity = Characters.FirstOrDefault(e => e.Position == point);
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

        private static bool IsPassableTile(DungeonTile dungeonTile)
        {
            return dungeonTile != DungeonTile.Stone && dungeonTile != DungeonTile.Wall;
        }

        public void PlacePlayer(Character player)
        {
            characters.Add(player);
        }

        public Character RemovePlayer(Character player)
        {
            characters.Remove(player);
            return player;
        }

        public void GenerateMonsters(bool isFirstEnter)
        {
            int min = isFirstEnter ? Settings.MinMonsters : (int) Math.Ceiling(Settings.MinMonsters / 4d);
            int max = isFirstEnter ? Settings.MaxMonsters : (int) Math.Ceiling(Settings.MaxMonsters / 2d);

            int count = DungeonGame.Random.Next(min, max);
            for (int i = 0; i < count; i++)
            {
                characters.Add(MonsterFactory.GetRandom(RandomFreePoint, Settings.FloorIndex));
            }
        }

        public bool CanEntityMove(Character entity, Direction direction)
        {
            var newPos = Character.NewPosition(direction, entity.Position);
            return Tiles[newPos.X, newPos.Y] == DungeonTile.Floor;
        }
        
        public bool IsTransparent(int x, int y)
        {
            return !(Doors.Find(d => !d.IsOpen && d.Position.X == x && d.Position.Y == y) != null ||
                     Tiles[x, y] == DungeonTile.Stone ||
                     Tiles[x, y] == DungeonTile.Wall ||
                     characters.Find(c => c.Position.X == x && c.Position.Y == y) != null);
        }
    }
}
