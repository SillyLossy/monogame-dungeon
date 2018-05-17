using System;
using System.Collections.Generic;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Common;
using SpecialAdventure.Core.World.Levels;
using SpecialAdventure.Core.World.Tiles;

namespace SpecialAdventure.Core.World.Generators
{
    public class CaveGenerator : LevelGenerator
    {
        public CaveGenerator(World world, int seed, int minDepth, int maxDepth) : base(world, seed, minDepth, maxDepth)
        {

        }

        public override Level Generate(Warp returnWarp)
        {
            int depth = DepthRange.Value;
            var floors = new List<Floor>();
            var id = Guid.NewGuid();
            var location = new Location(LocationType.Cave, id, 0);

            for (int i = 0; i < depth; i++)
            {
                floors.Add(GenerateFloor(CaveFloorSettings.GetSettings(i, Random)));
            }

            return new Level(location, floors.AsReadOnly());
        }

        private Floor GenerateFloor(CaveFloorSettings settings)
        {
            //So, first we make the map
            var map = new Dictionary<Point, Tile>();
            //And randomly scatter solid blocks
            InitializeMap(map, settings);

            //Then, for a number of steps
            for (int i = 0; i < settings.NumberOfSteps; i++)
            {
                //We apply our simulation rules!
                map = DoSimulationStep(map, settings);
            }

            for (int y = 0; y < settings.Height; y++)
            {
                if (y == 0 || y == settings.Height - 1)
                {
                    for (int x = 0; x < settings.Width; x++)
                    {
                        map[new Point(x, y)] = Tile.Stone;
                    }
                    continue;
                }
                map[new Point(0, y)] = Tile.Stone;
                map[new Point(settings.Width - 1, y)] = Tile.Stone;
            }

            //And we're done!
            return new Floor(map, new Map<Point, Entity>(), settings);
        }

        private void InitializeMap(Dictionary<Point, Tile> map, CaveFloorSettings settings)
        {
            for (int x = 0; x < settings.Width; x++)
            {
                for (int y = 0; y < settings.Height; y++)
                {
                    map[new Point(x, y)] = Tile.Floor;
                }
            }

            for (int x = 0; x < settings.Width; x++)
            {
                for (int y = 0; y < settings.Height; y++)
                {
                    //Here we use our ChanceToStartAlive variable
                    if (Random.NextDouble() < settings.ChanceToStartAlive)
                        //We're using numbers, not booleans, to decide if something is solid here. 0 = not solid
                        map[new Point(x, y)] = Tile.Stone;
                }
            }
        }

        private Dictionary<Point, Tile> DoSimulationStep(Dictionary<Point, Tile> map, CaveFloorSettings settings)
        {
            //Here's the new map we're going to copy our data into
            var newmap = new Dictionary<Point, Tile>();
            for (int x = 0; x < settings.Width; x++)
            {
                for (int y = 0; y < settings.Height; y++)
                {
                    //Count up the neighbours
                    int nbs = CountAliveNeighbours(map, x, y, settings);
                    //If the tile is currently solid
                    if (map[new Point(x, y)] == Tile.Stone)
                    {
                        //See if it should die
                        if (nbs < settings.DeathLimit)
                        {
                            newmap[new Point(x, y)] = Tile.Floor;
                        }
                        //Otherwise keep it solid
                        else
                        {
                            newmap[new Point(x, y)] = Tile.Stone;
                        }
                    }
                    //If the tile is currently empty
                    else
                    {
                        //See if it should become solid
                        if (nbs > settings.BirthLimit)
                        {
                            newmap[new Point(x, y)] = Tile.Stone;
                        }
                        else
                        {
                            newmap[new Point(x, y)] = Tile.Floor;
                        }
                    }
                }
            }

            return newmap;
        }

        //This function counts the number of solid neighbours a tile has
        private int CountAliveNeighbours(Dictionary<Point, Tile> map, int x, int y, CaveFloorSettings settings)
        {
            int count = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int nbX = i + x;
                    int nbY = j + y;
                    if (i == 0 && j == 0)
                    {
                    }
                    //If it's at the edges, consider it to be solid (you can try removing the count = count + 1)
                    else if (nbX < 0 || nbY < 0 || nbX >= settings.Width || nbY >= settings.Height)
                    {
                        count = count + 1;
                    }
                    else if (map[new Point(nbX, nbY)] == Tile.Stone)
                    {
                        count = count + 1;
                    }
                }
            }
            return count;
        }
    }
}