using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public class MonsterFactory
    {
        private int MinFloor { get; set; }
        private int MaxFloor { get; set; }
        private string Name { get; set; }
        private string TextureKey { get; set; }
        private Range Level { get; set; }
        private Range Strength { get; set; }
        private Range Perception { get; set; }
        private Range Agility { get; set; }
        private Range Charisma { get; set; }
        private Range Endurance { get; set; }
        private Range Intelligence { get; set; }
        private Range Luck { get; set; }
        private Range ExperienceReward { get; set; }
        public List<ItemFactory> Weapons { get; set; }
        public List<ItemFactory> Armor { get; set; }
        public List<ItemFactory> LootList { get; set; }

        private static readonly IReadOnlyList<MonsterFactory> prefabs = new List<MonsterFactory>
        {
            new MonsterFactory
            {
                Name = "Goblin",
                TextureKey = "Textures/Goblin",
                MinFloor = 0,
                MaxFloor = 5,
                Level = new Range(1),
                ExperienceReward = new Range(50, 100),
                Strength = new Range(1, 3),
                Perception = new Range(2, 3),
                Endurance = new Range(3, 4),
                Charisma = new Range(1),
                Intelligence = new Range(1),
                Agility = new Range(3, 4),
                Luck = new Range(1, 10),
                LootList = new List<ItemFactory>(),
                Weapons = new List<ItemFactory>
                {
                    new WeaponFactory
                    {

                    }
                },
            },
            new MonsterFactory
            {
                Name = "Tortoise",
                TextureKey = "Textures/Tortoise",
                MinFloor = 0,
                MaxFloor = 5,
                Level = new Range(1),
                ExperienceReward = new Range(50, 75),
                Strength = new Range(3, 4),
                Perception = new Range(2, 3),
                Endurance = new Range(5, 6),
                Charisma = new Range(1),
                Intelligence = new Range(1, 3),
                Agility = new Range(1, 2),
                Luck = new Range(1, 10),
                LootList = new List<ItemFactory>()
            }
        }.AsReadOnly();


        public Character Construct(Point position)
        {
            return new Monster(
                Name,
                TextureKey,
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
                position)
            {
                Experience = Character.GetExpCap(Level.Value) + 1,
                ExperienceReward = ExperienceReward.Value
            };
        }

        public static Character GetRandom(Point position, int floorIndex)
        {
            var acceptablePrefabs = prefabs.Where(p => p.MinFloor >= floorIndex && p.MaxFloor <= floorIndex).ToList();
            var prefab = acceptablePrefabs.Count > 0 
                ? acceptablePrefabs[DungeonGame.Random.Next(0, acceptablePrefabs.Count)]
                : prefabs[DungeonGame.Random.Next(0, prefabs.Count)];

            return prefab.Construct(position);
        }
    }

    public class WeaponFactory : ItemFactory
    {
        public Range Damage { get; set; }
        public Skill GovernedSkill { get; set; }
        public int ActionPointsRequired { get; set; }

        public override Item Construct()
        {
            return new Weapon(Name, Weight, BasePrice, Damage, GovernedSkill, ActionPointsRequired, TextureKey);
        }
    }

    public class ItemFactory
    {
        public string Name { get; set; }
        public int BasePrice { get; set; }
        public double Weight { get; set; }
        public string TextureKey { get; set; }

        public virtual Item Construct()
        {
            return new Item(Name, Weight, BasePrice, TextureKey);
        }
    }
}
