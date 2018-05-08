using System;
using System.Collections.Generic;
using System.Linq;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.World.Levels;
using SpecialAdventure.Core.World.Tiles;

namespace SpecialAdventure.Core.World.Generators
{
    ///<summary>
    /// generator-1.py, a simple python dungeon generator by
    /// James Spencer jamessp [at] gmail.com
    /// ---
    /// Rewritten to C#
    ///</summary>
    public class DungeonGenerator : LevelGenerator
    {
        public DungeonGenerator(int seed, int minDepth, int maxDepth) : base(seed, minDepth, maxDepth)
        {
        }

        public override AbstractLevel Generate()
        {
            int depth = DepthRange.Value;
            var floors = new List<Floor>();

            for (int i = 0; i < depth; i++)
            {
                floors.Add(GenerateFloor(DungeonFloorSettings.GetSettings(i, Random)));
            }

            return new Cave(floors.AsReadOnly());
        }

        public Floor GenerateFloor(DungeonFloorSettings settings)
        {
            var level = new Dictionary<Point, Tile>();
            var floor = new Floor(level, settings);

            // build an empty dungeon, blank the room and corridor lists
            for (int i = 0; i < settings.Width; i++)
            {
                for (int j = 0; j < settings.Height; j++)
                {
                    level[new Point(i, j)] = Tile.Stone;
                }
            }

            var roomList = new List<Rectangle>();
            var corridorList = new List<List<Point>>();

            int maxIters = settings.MaxRooms * 5;
            for (int i = 0; i < maxIters; i++)
            {
                var tmpRoom = GenerateRoom(settings);
                if (settings.RoomsOverlap || roomList.Count == 0)
                {
                    roomList.Add(tmpRoom);
                }
                else
                {
                    tmpRoom = GenerateRoom(settings);
                    var tmpRoomList = roomList.ToList();
                    if (RoomsOverlapping(tmpRoom, tmpRoomList) == false)
                    {
                        roomList.Add(tmpRoom);
                    }
                }

                if (roomList.Count >= settings.MaxRooms)
                {
                    break;
                }
            }

            // connect the rooms
            for (int i = 0; i < roomList.Count - 1; i++)
            {
                JoinRooms(settings, corridorList, roomList[i], roomList[i + 1]);
            }

            // do the Random joins
            for (int i = 0; i < settings.RandomConnections; i++)
            {
                var room1 = roomList[Random.Next(roomList.Count)];
                var room2 = roomList[Random.Next(roomList.Count)];
                JoinRooms(settings, corridorList, room1, room2);
            }

            // do the spurs
            for (int i = 0; i < settings.RandomSpurs; i++)
            {
                var room1 = new Rectangle { X = Random.Next(2, settings.Width - 2), Y = Random.Next(2, settings.Height - 2), W = 1, H = 1 };
                var room2 = roomList[Random.Next(roomList.Count)];
                JoinRooms(settings, corridorList, room1, room2);
            }

            // fill the map
            // paint rooms
            foreach (var room in roomList)
            {
                for (int x = 0; x < room.W; x++)
                {
                    for (int y = 0; y < room.H; y++)
                    {
                        level[new Point(room.X + x, room.Y + y)] = Tile.Floor;
                    }
                }
            }

            // paint corridors
            foreach (var corridor in corridorList)
            {
                int x1 = corridor[0].X;
                int y1 = corridor[0].Y;
                int x2 = corridor[1].X;
                int y2 = corridor[1].Y;
                for (int w = 0; w < (Math.Abs(x1 - x2) + 1); w++)
                {
                    for (int h = 0; h < (Math.Abs(y1 - y2) + 1); h++)
                    {
                        level[new Point(w + Math.Min(x1, x2), h + Math.Min(y1, y2))] = Tile.Floor;
                    }
                }

                if (corridor.Count == 3)
                {
                    int x3 = corridor[2].X;
                    int y3 = corridor[2].Y;
                    for (int w = 0; w < (Math.Abs(x2 - x3) + 1); w++)
                    {
                        for (int h = 0; h < (Math.Abs(y2 - y3) + 1); h++)
                        {
                            level[new Point(w + Math.Min(x2, x3), h + Math.Min(y2, y3))] = Tile.Floor;
                        }
                    }
                }
            }

            // paint the walls
            for (int col = 1; col < settings.Width - 1; col++)
            {
                for (int row = 1; row < settings.Height - 1; row++)
                {
                    if (level[new Point(col, row)] == Tile.Floor)
                    {
                        if (level[new Point(col - 1, row - 1)] == Tile.Stone)
                        {
                            level[new Point(col - 1, row - 1)] = Tile.Wall;
                        }

                        if (level[new Point(col, row - 1)] == Tile.Stone)
                        {
                            level[new Point(col, row - 1)] = Tile.Wall;
                        }

                        if (level[new Point(col + 1, row - 1)] == Tile.Stone)
                        {
                            level[new Point(col + 1, row - 1)] = Tile.Wall;
                        }

                        if (level[new Point(col - 1, row)] == Tile.Stone)
                        {
                            level[new Point(col - 1, row)] = Tile.Wall;
                        }

                        if (level[new Point(col + 1, row)] == Tile.Stone)
                        {
                            level[new Point(col + 1, row)] = Tile.Wall;
                        }

                        if (level[new Point(col - 1, row + 1)] == Tile.Stone)
                        {
                            level[new Point(col - 1, row + 1)] = Tile.Wall;
                        }

                        if (level[new Point(col, row + 1)] == Tile.Stone)
                        {
                            level[new Point(col, row + 1)] = Tile.Wall;
                        }

                        if (level[new Point(col + 1, row + 1)] == Tile.Stone)
                        {
                            level[new Point(col + 1, row + 1)] = Tile.Wall;
                        }
                    }
                }
            }

            var doors = new Dictionary<Point, Door>();

            // paint doors
            foreach (var room in roomList)
            {
                // paint top doors
                for (int x = 1; x < room.W - 1; x++)
                {
                    int top1y = room.Y - 1;
                    var position = new Point(room.X + x, top1y);
                    if (top1y <= 0 || doors.ContainsKey(position))
                    {
                        break;
                    }
                    if (level[new Point(room.X + x, top1y)] == Tile.Floor &&
                        level[new Point(room.X + x + 1, top1y)] != Tile.Floor &&
                        level[new Point(room.X + x - 1, top1y)] != Tile.Floor)
                    {
                        if (Random.NextDouble() > 0.25)
                        {
                            doors.Add(position, new Door(position));
                        }
                    }
                }
                // paint bottom doors
                for (int x = 1; x < room.W - 1; x++)
                {
                    int bottom1y = room.Y + room.H;
                    var position = new Point(room.X + x, bottom1y);
                    if (bottom1y >= settings.Height || doors.ContainsKey(position))
                    {
                        break;
                    }
                    if (level[new Point(room.X + x, bottom1y)] == Tile.Floor &&
                        level[new Point(room.X + x + 1, bottom1y)] != Tile.Floor &&
                        level[new Point(room.X + x - 1, bottom1y)] != Tile.Floor)
                    {
                        if (Random.NextDouble() > 0.25)
                        {
                            doors.Add(position, new Door(position));
                        }
                    }
                }
                // paint left doors
                for (int y = 1; y < room.H - 1; y++)
                {
                    int left1x = room.X - 1;
                    var position = new Point(left1x, room.Y + y);
                    if (left1x <= 0 || doors.ContainsKey(position))
                    {
                        break;
                    }

                    if (level[new Point(left1x, room.Y + y)] == Tile.Floor &&
                        level[new Point(left1x, room.Y + y + 1)] != Tile.Floor &&
                        level[new Point(left1x, room.Y + y - 1)] != Tile.Floor)
                    {
                        if (Random.NextDouble() > 0.25)
                        {
                            doors.Add(position, new Door(position));
                        }
                    }
                }
                // paint right doors
                for (int y = 1; y < room.H - 1; y++)
                {
                    int right1x = room.X + room.W;
                    var position = new Point(right1x, room.Y + y);
                    if (right1x <= 0 || doors.ContainsKey(position))
                    {
                        break;
                    }

                    if (level[new Point(right1x, room.Y + y)] == Tile.Floor &&
                        level[new Point(right1x, room.Y + y + 1)] != Tile.Floor &&
                        level[new Point(right1x, room.Y + y - 1)] != Tile.Floor)
                    {
                        if (Random.NextDouble() > 0.25)
                        {
                            doors.Add(position, new Door(position));
                        }
                    }
                }
            }

            floor.Doors = doors.Values.ToList();

            Point exit = null;
            Point entrance = null;
            do
            {
                try
                {
                    var entranceVariant = floor.RandomFreePoint;
                    var exitVariant = floor.RandomFreePoint;

                    if (entranceVariant == exitVariant ||
                        PathFinder.AStar(floor, entranceVariant, exitVariant, ignoreEntities: true) == null ||
                        !floor.GetNeighbors(entranceVariant).Any() ||
                        floor.Doors.Find(d => d.Position == exitVariant || d.Position == entranceVariant) != null)
                    {
                        continue;
                    }

                    exit = exitVariant;
                    entrance = entranceVariant;
                }
                catch
                {
                    // ignored
                }
            } while (exit == null || entrance == null);

            floor.Tiles[exit] = Tile.LadderDown;
            floor.Tiles[entrance] = Tile.LadderUp;

            floor.GenerateMonsters(isFirstEnter: true);

            return floor;
        }
        
        private Rectangle GenerateRoom(DungeonFloorSettings settings)
        {
            int w = Random.Next(settings.RoomSize.Min, settings.RoomSize.Max);
            int h = Random.Next(settings.RoomSize.Min, settings.RoomSize.Max);
            int x = Random.Next(1, settings.Width - w - 1);
            int y = Random.Next(1, settings.Height - h - 1);
            return new Rectangle
            {
                W = w,
                H = h,
                X = x,
                Y = y
            };
        }

        private static bool RoomsOverlapping(Rectangle room, IEnumerable<Rectangle> rooms)
        {
            foreach (var currentRoom in rooms)
            {
                if (room.X < currentRoom.X + currentRoom.W && currentRoom.X < room.X + room.W &&
                    room.Y < currentRoom.Y + currentRoom.H && currentRoom.Y < room.Y + room.H)
                {
                    return true;
                }
            }

            return false;
        }

        private List<Point> CorridorBetweenPoints(DungeonFloorSettings settings, int x1, int y1, int x2, int y2, Join joinType = Join.Either)
        {
            if (x1 == x2 && y1 == y2 || x1 == x2 || y1 == y2)
            {
                return new List<Point> { new Point(x1, y1), new Point(x2, y2) };
            }

            // 2 Corridors
            // NOTE: Never randomly choose a join that will go out of bounds
            // when the walls are added.
            Join join = joinType;
            if (joinType == Join.Either)
            {
                if (new[] { 0, 1 }.Intersect(new[] { x1, x2, y1, y2 }).Any())
                {
                    join = Join.Bottom;
                }
                else if (new[] { settings.Width - 1, settings.Width - 2 }.Intersect(new[] { x1, x2 }).Any() ||
                         new[] { settings.Height - 1, settings.Height - 2 }.Intersect(new[] { y1, y2 }).Any())
                {
                    join = Join.Top;
                }
                else
                {
                    join = Random.NextDouble() > 0.5 ? Join.Top : Join.Bottom;
                }
            }

            if (join == Join.Top)
            {
                return new List<Point>
                {
                    new Point(x1, y1),
                    new Point(x1, y2),
                    new Point(x2, y2)
                };
            }
            else
            {
                return new List<Point>
                {
                    new Point(x1, y1),
                    new Point(x2, y1),
                    new Point(x2, y2)
                };
            }
        }

        private void JoinRooms(DungeonFloorSettings settings, List<List<Point>> corridorList, Rectangle room1, Rectangle room2, Join joinType = Join.Either)
        {
            // sort by the value of x
            var sortedRooms = new List<Rectangle> { room1, room2 };
            sortedRooms.Sort((r1, r2) => r1.X.CompareTo(r2.X));
            int x1 = sortedRooms[0].X;
            int y1 = sortedRooms[0].Y;
            int w1 = sortedRooms[0].W;
            int h1 = sortedRooms[0].H;
            int x1_2 = x1 + w1 - 1;
            int y1_2 = y1 + h1 - 1;

            int x2 = sortedRooms[1].X;
            int y2 = sortedRooms[1].Y;
            int w2 = sortedRooms[1].W;
            int h2 = sortedRooms[1].H;
            int x2_2 = x2 + w2 - 1;
            int y2_2 = y2 + h2 - 1;

            if (x1 < (x2 + w2) && x2 < (x1 + w1)) // overlapping on x
            {
                int jx1 = Random.Next(x2, x1_2);
                int jx2 = jx1;
                var tmpY = new List<int> { y1, y2, y1_2, y2_2 };
                tmpY.Sort();
                int jy1 = tmpY[1] + 1;
                int jy2 = tmpY[2] - 1;
                var corridors = CorridorBetweenPoints(settings, jx1, jy1, jx2, jy2);
                corridorList.Add(corridors);
            }
            else if (y1 < (y2 + h2) && y2 < (y1 + h1)) // overlapping on y
            {
                int jy1;
                int jy2;

                if (y2 > y1)
                {
                    jy1 = Random.Next(y2, y1_2);
                    jy2 = jy1;
                }
                else
                {
                    jy1 = Random.Next(y1, y2_2);
                    jy2 = jy1;
                }

                var tmpX = new List<int> { x1, x2, x1_2, x2_2 };
                tmpX.Sort();
                int jx1 = tmpX[1] + 1;
                int jx2 = tmpX[2] - 1;
                var corridors = CorridorBetweenPoints(settings, jx1, jy1, jx2, jy2);
                corridorList.Add(corridors);
            }
            else // no overlap
            {
                Join join = joinType;
                if (joinType == Join.Either)
                {
                    join = Random.NextDouble() > 0.5 ? Join.Top : Join.Bottom;
                }

                if (join == Join.Top)
                {
                    if (y2 > y1)
                    {
                        int jx1 = x1_2 + 1;
                        int jy1 = Random.Next(y1, y1_2);
                        int jx2 = Random.Next(x2, x2_2);
                        int jy2 = y2 - 1;
                        var corridors = CorridorBetweenPoints(settings, jx1, jy1, jx2, jy2, Join.Bottom);
                        corridorList.Add(corridors);
                    }
                    else
                    {
                        int jx1 = Random.Next(x1, x1_2);
                        int jy1 = y1 - 1;
                        int jx2 = x2 - 1;
                        int jy2 = Random.Next(y2, y2_2);
                        var corridors = CorridorBetweenPoints(settings, jx1, jy1, jx2, jy2, Join.Top);
                        corridorList.Add(corridors);
                    }
                }
                else if (join == Join.Bottom)
                {
                    if (y2 > y1)
                    {
                        int jx1 = Random.Next(x1, x1_2);
                        int jy1 = y1_2 + 1;
                        int jx2 = x2 - 1;
                        int jy2 = Random.Next(y2, y2_2);
                        var corridors = CorridorBetweenPoints(settings, jx1, jy1, jx2, jy2, Join.Top);
                        corridorList.Add(corridors);
                    }
                }
                else
                {
                    int jx1 = x1_2 + 1;
                    int jy1 = Random.Next(y1, y1_2);
                    int jx2 = Random.Next(x2, x2_2);
                    int jy2 = y2_2 + 1;
                    var corridors = CorridorBetweenPoints(settings, jx1, jy1, jx2, jy2, Join.Bottom);
                    corridorList.Add(corridors);
                }
            }
        }
    }
}