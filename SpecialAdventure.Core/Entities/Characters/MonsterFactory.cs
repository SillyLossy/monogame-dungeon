using System.Collections.Generic;
using System.Linq;
using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.Entities.Characters
{
    public class MonsterFactory
    {
        private int MinFloor { get; set; }
        private int MaxFloor { get; set; }
        private string Name { get; set; }
        private IntRange Level { get; set; }
        private IntRange Strength { get; set; }
        private IntRange Perception { get; set; }
        private IntRange Agility { get; set; }
        private IntRange Charisma { get; set; }
        private IntRange Endurance { get; set; }
        private IntRange Intelligence { get; set; }
        private IntRange Luck { get; set; }
        private IntRange ExperienceReward { get; set; }
        public List<int> Weapons { get; set; }
        public List<int> Armor { get; set; }
        public List<int> LootList { get; set; }
        private int SpriteId { get; set; }

        private static readonly IReadOnlyList<MonsterFactory> prefabs = new List<MonsterFactory>
        {
            new MonsterFactory
            {
                Name = "Goblin",
                SpriteId = 14,
                MinFloor = 0,
                MaxFloor = 5,
                Level = new IntRange(1),
                ExperienceReward = new IntRange(50, 100),
                Strength = new IntRange(1, 3),
                Perception = new IntRange(2, 3),
                Endurance = new IntRange(3, 4),
                Charisma = new IntRange(1),
                Intelligence = new IntRange(1),
                Agility = new IntRange(3, 4),
                Luck = new IntRange(1, 10)
            },
            new MonsterFactory
            {
                Name = "Green Tortoise",
                SpriteId = 17,
                MinFloor = 0,
                MaxFloor = 5,
                Level = new IntRange(1),
                ExperienceReward = new IntRange(50, 75),
                Strength = new IntRange(3, 4),
                Perception = new IntRange(2, 3),
                Endurance = new IntRange(5, 6),
                Charisma = new IntRange(1),
                Intelligence = new IntRange(1, 3),
                Agility = new IntRange(1, 2),
                Luck = new IntRange(1, 10)
            }
        }.AsReadOnly();


        public Character Construct(Point position)
        {
            return new Monster(
                Name,
                new PrimaryAttributes
                {
                    Strength = Strength.Value,
                    Perception = Perception.Value,
                    Endurance = Endurance.Value,
                    Charisma = Charisma.Value,
                    Intelligence = Intelligence.Value,
                    Agility = Agility.Value,
                    Luck = Luck.Value
                },
                position,
                SpriteId)
            {
                Experience = Character.GetExperienceCap(Level.Value) + 1,
                ExperienceReward = ExperienceReward.Value
            };
        }

        public static Character GetRandom(Point position, int floorIndex)
        {
            var acceptablePrefabs = prefabs.Where(p => p.MinFloor >= floorIndex && p.MaxFloor <= floorIndex).ToList();
            var prefab = acceptablePrefabs.Count > 0 
                ? acceptablePrefabs[RandomHelper.Random.Next(0, acceptablePrefabs.Count)]
                : prefabs[RandomHelper.Random.Next(0, prefabs.Count)];

            return prefab.Construct(position);
        }
    }
}
