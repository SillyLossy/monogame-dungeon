using System.Collections.Generic;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Characters;
using SpecialAdventure.Core.Entities.Common;
using SpecialAdventure.Core.World.Generators;
using SpecialAdventure.Core.World.Tiles;

namespace SpecialAdventure.Core.World
{
    public class Floor
    {
        public Dictionary<Point, Tile> Tiles { get; }

        public Point RandomFreePoint
        {
            get
            {
                Point point = null;
                for (int i = 0; i < Settings.Width * Settings.Height; i++)
                {
                    int x = RandomHelper.Random.Next(Settings.Width);
                    int y = RandomHelper.Random.Next(Settings.Height);
                    var randomPoint = new Point(x, y);

                    if (!IsPointPassable(randomPoint))
                    {
                        continue;
                    }

                    point = randomPoint;
                    break;
                }

                return point;
            }
        }

        public Map<Point, Entity> Entities { get; }

        public AbstractFloorSettings Settings { get; }

        public Point EntrancePoint { get; }

        public Point ExitPoint { get; }
        
        public bool IsTransparent(Point point)
        {
            bool tileTransparent = Tiles[point].IsTransparent;

            if (Entities.Forward.Contains(point))
            {
                return tileTransparent && Entities.Forward[point].IsTransparent;
            }

            return tileTransparent;
        }
        
        public Floor(Dictionary<Point, Tile> tiles, Map<Point, Entity> entities, AbstractFloorSettings settings)
        {
            Tiles = tiles;
            Settings = settings;
            Entities = entities;
            var (entrance, exit) = GetWarpPoints();
            ExitPoint = exit;
            EntrancePoint = entrance;
        }

        private (Point Entrance, Point Exit) GetWarpPoints()
        {
            Point exit = null;
            Point entrance = null;
            do
            {
                var entranceVariant = RandomFreePoint;
                var exitVariant = RandomFreePoint;

                if (entranceVariant == exitVariant || PathFinder.AStar(this, entranceVariant, exitVariant, ignoreEntities: true) == null)
                {
                    continue;
                }

                exit = exitVariant;
                entrance = entranceVariant;

            } while (exit == null || entrance == null);

            return (Entrance: entrance, Exit: exit);
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
                    if (Tiles[point].IsPassable)
                    {
                        if (ignoreEntities == false)
                        {
                            if (Entities.Forward.Contains(point))
                            {
                                if (Entities.Forward[point].IsPassable)
                                {
                                    yield return point;
                                }
                            }
                            else
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

        public void GenerateMonsters(bool isFirstEnter)
        {
            int min = isFirstEnter ? Settings.Monsters.Min : (Settings.Monsters.Min / 8);
            int max = isFirstEnter ? Settings.Monsters.Max : (Settings.Monsters.Max / 8);

            int count = RandomHelper.Random.Next(min, max);
            for (int i = 0; i < count; i++)
            {
                Entities.Add(RandomFreePoint, MonsterFactory.GetRandom(RandomFreePoint, Settings.Depth));
            }
        }
        
        public bool IsPointPassable(Point point)
        {
            return Tiles[point].IsPassable && !Entities.Forward.Contains(point);
        }
    }
}
