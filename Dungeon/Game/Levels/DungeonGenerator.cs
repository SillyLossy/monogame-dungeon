using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Entities;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Levels
{
    ///<summary>
    /// generator-1.py, a simple python dungeon generator by
    /// James Spencer jamessp [at] gmail.com
    /// ---
    /// Rewritten to C#
    ///</summary>
    public static class DungeonGenerator
    {
        public static DungeonFloor GenerateFloor(int seed, FloorSettings settings)
        {
            var random = new Random(seed);

            var level = new DungeonTile[settings.Width, settings.Height];
            var floor = new DungeonFloor(level, settings);

            // build an empty dungeon, blank the room and corridor lists
            for (int i = 0; i < settings.Width; i++)
            {
                for (int j = 0; j < settings.Height; j++)
                {
                    level[i, j] = DungeonTile.Stone;
                }
            }

            var roomList = new List<Room>();
            var corridorList = new List<List<Tuple<int, int>>>();

            int maxIters = settings.MaxRooms * 5;
            for (int i = 0; i < maxIters; i++)
            {
                var tmpRoom = GenerateRoom(random, settings);
                if (settings.RoomsOverlap || roomList.Count == 0)
                {
                    roomList.Add(tmpRoom);
                }
                else
                {
                    tmpRoom = GenerateRoom(random, settings);
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
                JoinRooms(random, settings, corridorList, roomList[i], roomList[i + 1]);
            }

            // do the random joins
            for (int i = 0; i < settings.RandomConnections; i++)
            {
                var room1 = roomList[random.Next(roomList.Count)];
                var room2 = roomList[random.Next(roomList.Count)];
                JoinRooms(random, settings, corridorList, room1, room2);
            }

            // do the spurs
            for (int i = 0; i < settings.RandomSpurs; i++)
            {
                var room1 = new Room { X = random.Next(2, settings.Width - 2), Y = random.Next(2, settings.Height - 2), W = 1, H = 1 };
                var room2 = roomList[random.Next(roomList.Count)];
                JoinRooms(random, settings, corridorList, room1, room2);
            }

            // fill the map
            // paint rooms
            foreach (var room in roomList)
            {
                for (int x = 0; x < room.W; x++)
                {
                    for (int y = 0; y < room.H; y++)
                    {
                        level[room.X + x, room.Y + y] = DungeonTile.Floor;
                    }
                }
            }

            // paint corridors
            foreach (var corridor in corridorList)
            {
                int x1 = corridor[0].Item1;
                int y1 = corridor[0].Item2;
                int x2 = corridor[1].Item1;
                int y2 = corridor[1].Item2;
                for (int w = 0; w < (Math.Abs(x1 - x2) + 1); w++)
                {
                    for (int h = 0; h < (Math.Abs(y1 - y2) + 1); h++)
                    {
                        level[w + Math.Min(x1, x2), h + Math.Min(y1, y2)] = DungeonTile.Floor;
                    }
                }

                if (corridor.Count == 3)
                {
                    int x3 = corridor[2].Item1;
                    int y3 = corridor[2].Item2;
                    for (int w = 0; w < (Math.Abs(x2 - x3) + 1); w++)
                    {
                        for (int h = 0; h < (Math.Abs(y2 - y3) + 1); h++)
                        {
                            level[w + Math.Min(x2, x3), h + Math.Min(y2, y3)] = DungeonTile.Floor;
                        }
                    }
                }
            }

            // paint the walls
            for (int col = 1; col < settings.Width - 1; col++)
            {
                for (int row = 1; row < settings.Height - 1; row++)
                {
                    if (level[col, row] == DungeonTile.Floor)
                    {
                        if (level[col - 1, row - 1] == DungeonTile.Stone)
                            level[col - 1, row - 1] = DungeonTile.Wall;
                        
                        if (level[col, row - 1] == DungeonTile.Stone)
                            level[col, row - 1] = DungeonTile.Wall;
                        
                        if(level[col + 1, row - 1] == DungeonTile.Stone)
                            level[col + 1, row - 1] = DungeonTile.Wall;
                        
                        if(level[col - 1, row] == DungeonTile.Stone)
                            level[col - 1, row] = DungeonTile.Wall;
                        
                        if (level[col + 1, row] == DungeonTile.Stone)
                            level[col + 1, row] = DungeonTile.Wall;
                        
                        if (level[col - 1, row + 1] == DungeonTile.Stone)
                            level[col - 1, row + 1] = DungeonTile.Wall;
                        
                        if (level[col, row + 1] == DungeonTile.Stone)
                            level[col, row + 1] = DungeonTile.Wall;

                        if (level[col + 1, row + 1] == DungeonTile.Stone)
                            level[col + 1, row + 1] = DungeonTile.Wall;
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
                    if (level[room.X + x, top1y] == DungeonTile.Floor &&
                        level[room.X + x + 1, top1y] != DungeonTile.Floor &&
                        level[room.X + x - 1, top1y] != DungeonTile.Floor)
                    {
                        if (random.NextDouble() > 0.25)
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
                    if (level[room.X + x, bottom1y] == DungeonTile.Floor &&
                        level[room.X + x + 1, bottom1y] != DungeonTile.Floor &&
                        level[room.X + x - 1, bottom1y] != DungeonTile.Floor)
                    {
                        if (random.NextDouble() > 0.25)
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

                    if (level[left1x, room.Y + y] == DungeonTile.Floor &&
                        level[left1x, room.Y + y + 1] != DungeonTile.Floor &&
                        level[left1x, room.Y + y - 1] != DungeonTile.Floor)
                    {
                        if (random.NextDouble() > 0.25)
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

                    if (level[right1x, room.Y + y] == DungeonTile.Floor &&
                        level[right1x, room.Y + y + 1] != DungeonTile.Floor &&
                        level[right1x, room.Y + y - 1] != DungeonTile.Floor)
                    {
                        if (random.NextDouble() > 0.25)
                        {
                            doors.Add(position, new Door(position));
                        }
                    }
                }
            }

            floor.Doors = doors.Values.ToList();

            Point? exit = null;
            Point? entrance = null;
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

            floor.Tiles[exit.Value.X, exit.Value.Y] = DungeonTile.LadderDown;
            floor.Tiles[entrance.Value.X, entrance.Value.Y] = DungeonTile.LadderUp;

            floor.GenerateMonsters(isFirstEnter: true);

            return floor;
        }

        private class Room
        {
            public int W { get; set; }
            public int H { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        private static Room GenerateRoom(Random random, FloorSettings settings)
        {
            int w = random.Next(settings.MinRoomXy, settings.MaxRoomXy);
            int h = random.Next(settings.MinRoomXy, settings.MaxRoomXy);
            int x = random.Next(1, settings.Width - w - 1);
            int y = random.Next(1, settings.Height - h - 1);
            return new Room
            {
                W = w,
                H = h,
                X = x,
                Y = y
            };
        }

        private static bool RoomsOverlapping(Room room, IEnumerable<Room> rooms)
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

        private static List<Tuple<int, int>> CorridorBetweenPoints(Random random, FloorSettings settings, int x1, int y1, int x2, int y2, Join joinType = Join.Either)
        {
            if (x1 == x2 && y1 == y2 || x1 == x2 || y1 == y2)
            {
                return new List<Tuple<int, int>> { new Tuple<int, int>(x1, y1), new Tuple<int, int>(x2, y2) };
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
                    join = random.NextDouble() > 0.5 ? Join.Top : Join.Bottom;
                }
            }

            if (join == Join.Top)
            {
                return new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(x1, y1),
                    new Tuple<int, int>(x1, y2),
                    new Tuple<int, int>(x2, y2)
                };
            }
            else
            {
                return new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(x1, y1),
                    new Tuple<int, int>(x2, y1),
                    new Tuple<int, int>(x2, y2)
                };
            }
        }

        private static void JoinRooms(Random random, FloorSettings settings, List<List<Tuple<int,int>>> corridorList, Room room1, Room room2, Join joinType = Join.Either)
        {
            // sort by the value of x
            var sortedRooms = new List<Room> { room1, room2 };
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
                int jx1 = random.Next(x2, x1_2);
                int jx2 = jx1;
                var tmpY = new List<int> { y1, y2, y1_2, y2_2 };
                tmpY.Sort();
                int jy1 = tmpY[1] + 1;
                int jy2 = tmpY[2] - 1;
                var corridors = CorridorBetweenPoints(random, settings, jx1, jy1, jx2, jy2);
                corridorList.Add(corridors);
            }
            else if (y1 < (y2 + h2) && y2 < (y1 + h1)) // overlapping on y
            {
                int jy1;
                int jy2;

                if (y2 > y1)
                {
                    jy1 = random.Next(y2, y1_2);
                    jy2 = jy1;
                }
                else
                {
                    jy1 = random.Next(y1, y2_2);
                    jy2 = jy1;
                }

                var tmpX = new List<int> { x1, x2, x1_2, x2_2 };
                tmpX.Sort();
                int jx1 = tmpX[1] + 1;
                int jx2 = tmpX[2] - 1;
                var corridors = CorridorBetweenPoints(random, settings, jx1, jy1, jx2, jy2);
                corridorList.Add(corridors);
            }
            else // no overlap
            {
                Join join = joinType;
                if (joinType == Join.Either)
                {
                    join = random.NextDouble() > 0.5 ? Join.Top : Join.Bottom;
                }

                if (join == Join.Top)
                {
                    if (y2 > y1)
                    {
                        int jx1 = x1_2 + 1;
                        int jy1 = random.Next(y1, y1_2);
                        int jx2 = random.Next(x2, x2_2);
                        int jy2 = y2 - 1;
                        var corridors = CorridorBetweenPoints(random, settings, jx1, jy1, jx2, jy2, Join.Bottom);
                        corridorList.Add(corridors);
                    }
                    else
                    {
                        int jx1 = random.Next(x1, x1_2);
                        int jy1 = y1 - 1;
                        int jx2 = x2 - 1;
                        int jy2 = random.Next(y2, y2_2);
                        var corridors = CorridorBetweenPoints(random, settings, jx1, jy1, jx2, jy2, Join.Top);
                        corridorList.Add(corridors);
                    }
                }
                else if (join == Join.Bottom)
                {
                    if (y2 > y1)
                    {
                        int jx1 = random.Next(x1, x1_2);
                        int jy1 = y1_2 + 1;
                        int jx2 = x2 - 1;
                        int jy2 = random.Next(y2, y2_2);
                        var corridors = CorridorBetweenPoints(random, settings, jx1, jy1, jx2, jy2, Join.Top);
                        corridorList.Add(corridors);
                    }
                }
                else
                {
                    int jx1 = x1_2 + 1;
                    int jy1 = random.Next(y1, y1_2);
                    int jx2 = random.Next(x2, x2_2);
                    int jy2 = y2_2 + 1;
                    var corridors = CorridorBetweenPoints(random, settings, jx1, jy1, jx2, jy2, Join.Bottom);
                    corridorList.Add(corridors);
                }
            }
        }
    }
}