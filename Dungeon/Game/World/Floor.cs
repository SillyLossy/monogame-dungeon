using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Common;
using Dungeon.Game.Entities.Characters;
using Dungeon.Game.World.Generators;
using Dungeon.Game.World.Tiles;
using Newtonsoft.Json;

namespace Dungeon.Game.World
{
    public class Floor
    {
        public Dictionary<Point, Tile> Tiles { get; }

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

                    if (!IsTransparent(randomPoint))
                    {
                        continue;
                    }

                    point = randomPoint;
                    break;
                }

                return point ?? RandomNeighborOfTile(Tile.Floor);
            }
        }

        public IEnumerable<Character> Characters => characters;
        public AbstractFloorSettings Settings { get; }
        public List<Door> Doors { get; set; }

        public Point RandomEntranceNeighbor => RandomNeighborOfTile(Tile.LadderUp);
        public Point RandomExitNeighbor => RandomNeighborOfTile(Tile.LadderDown);

        private Point RandomNeighborOfTile(Tile tile)
        {
            Point entrancePoint = null;
            for (int x = 0; x < Settings.Width; x++)
            {
                for (int y = 0; y < Settings.Height; y++)
                {
                    var point = new Point(x, y);
                    if (Tiles[point] == tile)
                    {
                        entrancePoint = point;
                    }
                }
            }

            if (entrancePoint == null)
            {
                throw new ArgumentOutOfRangeException(nameof(tile));
            }

            var entranceNeighbors = GetNeighbors(entrancePoint).ToArray();
            return entranceNeighbors[DungeonGame.Random.Next(entranceNeighbors.Length)];
        }
        
        private readonly List<Character> characters;


        public Floor(Dictionary<Point, Tile> tiles, AbstractFloorSettings settings)
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
                    var point = new Point(x, y);
                    if (IsPassableTile(Tiles[point]))
                    {
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

        private static bool IsPassableTile(Tile tile)
        {
            return tile != Tile.Stone && tile != Tile.Wall;
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
            int min = isFirstEnter ? Settings.Monsters.Min : (Settings.Monsters.Min / 8);
            int max = isFirstEnter ? Settings.Monsters.Max : (Settings.Monsters.Max / 8);

            int count = DungeonGame.Random.Next(min, max);
            for (int i = 0; i < count; i++)
            {
                characters.Add(MonsterFactory.GetRandom(RandomFreePoint, Settings.Depth));
            }
        }

        public bool CanEntityMove(Character entity, Direction direction)
        {
            var newPos = Character.NewPosition(direction, entity.Position);
            return Tiles[newPos] == Tile.Floor;
        }
        
        public bool IsTransparent(Point point)
        {
            return !(Doors.Find(d => !d.IsOpen && d.Position.X == point.X && d.Position.Y == point.Y) != null ||
                     Tiles[point] == Tile.Stone ||
                     Tiles[point] == Tile.Wall ||
                     characters.Find(c => c.Position.X == point.X && c.Position.Y == point.Y) != null);
        }
    }
}
