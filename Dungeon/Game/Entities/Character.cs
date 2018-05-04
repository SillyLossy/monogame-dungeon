using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.FOV;
using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Dungeon.Game.Entities
{
    public abstract class Character : Entity
    {
        public int Experience { get; set; }

        private static readonly IReadOnlyList<int> LevelLookup = new[]
        {
            1000,
            3000,
            6000,
            10_000,
            15_000,
            21_000,
            28_000,
            36_000,
            45_000,
            55_000,
            66_000,
            78_000,
            91_000,
            105_000,
            120_000,
            136_000,
            153_000,
            171_000,
            190_000,
            210_000
        };

        [JsonIgnore]
        public int Level
        {
            get
            {

                for (var i = 0; i < LevelLookup.Count; i++)
                    if (Experience < LevelLookup[i])
                        return i + 1;

                return (int)(Math.Round(Math.Sqrt(Experience + 125) / (10 * Math.Sqrt(5))) + 0.5);
            }
        }

        public static int GetExpCap(int level)
        {
            if (level - 1 < LevelLookup.Count)
            {
                return LevelLookup[level - 1];
            }

            return (level * (level - 1) / 2) * 1000;
        }

        public LinkedList<Point> Path { get; protected set; }

        public PrimaryAttributes PrimaryAttributes { get; }

        [JsonIgnore]
        public int MaxHitPoints => 15 + PrimaryAttributes.Strength +
                                   2 * PrimaryAttributes.Endurance + Level * (PrimaryAttributes.Endurance / 2) + 2;

        public int HitPoints { get; set; }

        [JsonIgnore]
        public int MaxCarryWeight => 25 + PrimaryAttributes.Strength * 25;
        
        [JsonIgnore]
        public int SkillRate => 5 + PrimaryAttributes.Intelligence * 2;
        
        [JsonIgnore]
        private double CriticalChance => 0.01 * PrimaryAttributes.Luck;
        
        [JsonIgnore]
        public int Sequence => 2 * PrimaryAttributes.Perception;
        
        [JsonIgnore]
        public int ActionPoints => (int)Math.Floor((PrimaryAttributes.Agility / 2d) + 5);
        
        [JsonIgnore]
        public int MeleeDamage => Math.Max(1, PrimaryAttributes.Strength - 5);

        public IReadOnlyDictionary<Skill, int> Skills { get; }

        public string Name { get; }

        public int ArmorClass
        {
            get
            {
                int? equipedArmorClass = null;

                if (Inventory.Equipped.ContainsKey(EquipSlot.Armor))
                {
                    var equipedArmor = Inventory.Equipped[EquipSlot.Armor] as Armor;
                    equipedArmorClass = equipedArmor?.ArmorClass;
                }

                return PrimaryAttributes.Agility + equipedArmorClass.GetValueOrDefault(0);
            }
        }

        public bool HasNextStep => Path != null && Path.Count > 0;

        public Inventory Inventory { get; } = new Inventory();

        public Character(string name, string textureKey, PrimaryAttributes primaryAttributes, Point initialPosition) : base(textureKey, initialPosition)
        {
            Name = name;
            PrimaryAttributes = primaryAttributes;
            Skills = GenerateSkills(primaryAttributes);
            Path = new LinkedList<Point>();
            HitPoints = MaxHitPoints;
        }

        private static Dictionary<Skill, int> GenerateSkills(PrimaryAttributes attributes)
        {
            return new Dictionary<Skill, int>
            {
                [Skill.Unarmed] = 65 + ((attributes.Agility + attributes.Strength) / 2),
                [Skill.Throwing] = 40 + attributes.Agility + attributes.Perception,
                [Skill.BluntWeapon] = 35 + (attributes.Strength * 2),
                [Skill.LongBlade] = 30 + ((attributes.Agility + attributes.Strength) / 2),
                [Skill.Marksman] = 40 + (attributes.Agility / 2) + attributes.Perception,
                [Skill.ShortBlade] = 45 + attributes.Agility,
                [Skill.Axe] = 35 + (attributes.Agility / 2) + attributes.Strength,
                [Skill.Spear] = 30 + (attributes.Agility * 2)
            };
        }
        
        public HashSet<Point> SeenPoints { get; set; } = new HashSet<Point>();

        [JsonIgnore]
        public int SightRadius => (int)Math.Floor(2 + PrimaryAttributes.Perception * 2d);

        [JsonIgnore]
        public Armor EquippedArmor => Inventory.Equipped.ContainsKey(EquipSlot.Armor)
            ? Inventory.Equipped[EquipSlot.Armor] as Armor
            : Armor.NoArmor;

        [JsonIgnore]
        public Weapon EquippedWeapon => Inventory.Equipped.ContainsKey(EquipSlot.Weapon)
            ? Inventory.Equipped[EquipSlot.Armor] as Weapon
            : Weapon.UnarmedWeapon(MeleeDamage);

        public HashSet<Point> GetVisiblePoints(DungeonFloor parent)
        {
            var algorithm = new PermissiveFov(parent.Settings.Width, parent.Settings.Height, parent.IsTransparent);
            var points = algorithm.Compute(Position.X, Position.Y, SightRadius);

            SeenPoints.UnionWith(points);
            return points;
        }

        // We can't move to non-floor tile directly, so move to nearest neighbor and then move to entity
        protected LinkedList<Point> FindPathToEntity(DungeonFloor parent, Point target)
        {
            // find a shortest path to entity and move to it
            LinkedList<Point> shortestNeighborPath = null;

            foreach (var neighbor in parent.GetNeighbors(target))
            {
                // can't move to an unknown tile
                if (!SeenPoints.Contains(neighbor)) continue;

                var path = PathFinder.AStar(parent, Position, neighbor);

                if (path == null) continue;

                if (shortestNeighborPath == null || path.Count < shortestNeighborPath.Count)
                {
                    shortestNeighborPath = path;
                    shortestNeighborPath.AddLast(target);
                }
            }

            return shortestNeighborPath;
        }

        public void MoveTo(DungeonFloor parent, Point target)
        {
            if (Position == target) return;

            LinkedList<Point> path = null;
            var door = parent.Doors.FirstOrDefault(d => !d.IsOpen && d.Position == target);
            var character = parent.Characters.FirstOrDefault(d => d.Position == target);
            if (door != null)
            {
                // if door is neighbor, just open it
                if (parent.GetNeighbors(door.Position, true).Any(neighbor => neighbor == Position))
                {
                    door.Open();
                    return;
                }

                path = FindPathToEntity(parent, door.Position);
            }
            else if (character != null)
            {
                if (parent.GetNeighbors(character.Position, true).Any(neighbor => neighbor == Position))
                {
                    Attack(character);
                    return;
                }
            }
            else
            {
                path = PathFinder.AStar(parent, Position, target);

            }
            
            if (path != null)
            {
                Path = path;
            }
        }

        public abstract void Update(DungeonGameState state);

        public void Step(DungeonFloor parent)
        {
            if (HasNextStep)
            {
                var newPos = Path.First.Value;
                Path.RemoveFirst();
                CheckForDoor(parent, newPos);

                Position = newPos;
            }
        }

        public void MoveTo(DungeonFloor parent, Direction direction)
        {
            var oldPos = Position;
            var newPos = NewPosition(direction, oldPos);

            CheckForDoor(parent, newPos);

            Position = newPos;
        }

        protected static void CheckForDoor(DungeonFloor parent, Point newPos)
        {
            var closedDoor = parent.Doors.FirstOrDefault(d => !d.IsOpen && d.Position == newPos);
            closedDoor?.Open();
        }

        public static Point NewPosition(Direction direction, Point oldPos)
        {
            switch (direction)
            {
                case Direction.South:
                    return new Point(oldPos.X, oldPos.Y + 1);
                case Direction.North:
                    return new Point(oldPos.X, oldPos.Y - 1);
                case Direction.West:
                    return new Point(oldPos.X - 1, oldPos.Y);
                case Direction.East:
                    return new Point(oldPos.X + 1, oldPos.Y);
                case Direction.NorthEast:
                    return new Point(oldPos.X + 1, oldPos.Y - 1);
                case Direction.NorthWest:
                    return new Point(oldPos.X - 1, oldPos.Y - 1);
                case Direction.SouthEast:
                    return new Point(oldPos.X + 1, oldPos.Y + 1);
                case Direction.SouthWest:
                    return new Point(oldPos.X - 1, oldPos.Y + 1);
                case Direction.None:
                    return new Point(oldPos.X, oldPos.Y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public virtual AttackResult Attack(Character target)
        {
            Weapon weapon = EquippedWeapon;

            int skillValue = Skills[weapon.GovernedSkill];
            double chanceToHit = AdjustHitChance(skillValue - target.ArmorClass);

            // attack on each turn will be blown at least once
            int blowsCount = Math.Max(1, (ActionPoints / weapon.ActionPointsRequired));

            var blowsList = new List<AttackResult.Blow>();

            for (int i = 0; i < blowsCount; i++)
            {
                bool isSuccessfulBlow = DungeonGame.Random.NextDouble() <= chanceToHit;
                if (!isSuccessfulBlow)
                {
                    blowsList.Add(new AttackResult.Blow { IsMiss = true });
                    continue;
                }

                int rawDamage = weapon.Damage.Value;

                // TODO: Implement ammo modification later
                int DM = 1;
                double DRM = 0;
                int DT = target.EquippedArmor.DamageThreshold;
                double DR = target.EquippedArmor.DamageResistance;

                int finalDamage = (int)Math.Floor(((rawDamage * DM) - DT) - Math.Floor((DR + DRM) * ((rawDamage * DM) - DT)));

                bool isCriticalHit = DungeonGame.Random.NextDouble() <= CriticalChance;
                if (isCriticalHit)
                {
                    finalDamage *= 3;
                }

                target.RemoveHitPoints(finalDamage);

                blowsList.Add(new AttackResult.Blow {Damage = finalDamage, IsCriticalHit = isCriticalHit});

                if (target.IsDead)
                {
                    break;
                }
            }

            return new AttackResult { Attacker = this, Target = target, Blows = blowsList };
        }

        public void RemoveHitPoints(int decrement)
        {
            HitPoints -= Math.Max(decrement, 0);
        }

        public void AddExperience(int increment)
        {
            int prevLevel = Level;
            Experience += Math.Max(increment, 0);
            int newLevel = Level;
            UnallocatedSkillPoints += (newLevel - prevLevel) * SkillRate;
        }

        public int UnallocatedSkillPoints { get; set; } = 0;

        public int ExperienceReward { get; set; } = 0;

        public bool IsDead => HitPoints <= 0;
        
        private static double AdjustHitChance(int rawChance)
        {
            if (rawChance < 5)
            {
                rawChance = 5;
            }

            if (rawChance > 95)
            {
                rawChance = 95;
            }

            return rawChance / 100d;
        }
    }

    public class AttackResult
    {
        public class Blow
        {
            public int Damage { get; set; }
            public bool IsCriticalHit { get; set; }
            public bool IsMiss { get; set; }
        }

        public Character Attacker { get; set; }
        public Character Target { get; set; }
        public List<Blow> Blows { get; set; }
    }
}