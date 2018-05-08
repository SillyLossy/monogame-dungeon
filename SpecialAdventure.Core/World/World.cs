using System;
using System.Collections.Generic;
using SpecialAdventure.Core.World.Generators;
using SpecialAdventure.Core.World.Levels;

namespace SpecialAdventure.Core.World
{
    public class World
    {
        private readonly Random random;

        private readonly LevelGenerator islandGenerator;

        private readonly LevelGenerator caveGenerator;

        private readonly LevelGenerator dungeonGenerator;

        public Dictionary<Guid, AbstractLevel> Dungeons { get; }

        public Dictionary<Guid, AbstractLevel> Caves { get; }

        public Dictionary<Guid, AbstractLevel> Islands { get; }

        public Guid GenerateIsland()
        {
            var id = Guid.NewGuid();
            var island = islandGenerator.Generate();
            Islands[id] = island;
            return id;
        }

        public Guid GenerateCave()
        {
            var id = Guid.NewGuid();
            var cave = caveGenerator.Generate();
            Caves[id] = cave;
            return id;
        }

        public Guid GenerateDungeon()
        {
            var id = Guid.NewGuid();
            var dungeon = dungeonGenerator.Generate();
            Dungeons[id] = dungeon;
            return id;
        }

        public World(int seed)
        {
            random = new Random(seed);

            islandGenerator = new IslandGenerator(random.Next());
            caveGenerator = new CaveGenerator(random.Next(), 1, 15);
            dungeonGenerator = new DungeonGenerator(random.Next(), 10, 15);

            Islands = new Dictionary<Guid, AbstractLevel>();
            Caves = new Dictionary<Guid, AbstractLevel>();
            Dungeons = new Dictionary<Guid, AbstractLevel>();
        }
        
    }
}
