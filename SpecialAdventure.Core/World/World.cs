using System;
using System.Collections.Generic;
using SpecialAdventure.Core.World.Generators;
using SpecialAdventure.Core.World.Levels;

namespace SpecialAdventure.Core.World
{
    public class World
    {
        public readonly LevelGenerator IslandGenerator;

        public readonly LevelGenerator CaveGenerator;

        public readonly LevelGenerator DungeonGenerator;

        public List<Level> Levels { get; }

        public Location InitialLocation { get; } 
        
        public World(int seed)
        {
            var random = new Random(seed);

            IslandGenerator = new IslandGenerator(this, random.Next());
            CaveGenerator = new CaveGenerator(this, random.Next(), 1, 15);
            DungeonGenerator = new DungeonGenerator(this, random.Next(), 10, 15);
            Levels = new List<Level>();
            var firstIsland = IslandGenerator.Generate(null);
            InitialLocation = firstIsland.Location;
            Levels.Add(firstIsland);
        }
    }
}
