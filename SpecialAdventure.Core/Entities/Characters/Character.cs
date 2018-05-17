using System;
using System.Collections.Generic;
using System.Linq;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Common;
using SpecialAdventure.Core.Entities.Items;
using SpecialAdventure.Core.FOV;
using SpecialAdventure.Core.Log;
using SpecialAdventure.Core.World;

namespace SpecialAdventure.Core.Entities.Characters
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

        public int Level
        {
            get
            {

                for (var i = 0; i < LevelLookup.Count; i++)
                {
                    if (Experience < LevelLookup[i])
                    {
                        return i + 1;
                    }
                }

                return (int)(Math.Round(Math.Sqrt(Experience + 125) / (10 * Math.Sqrt(5))) + 0.5);
            }
        }

        public static int GetExperienceCap(int level)
        {
            if (level - 1 < LevelLookup.Count)
            {
                return LevelLookup[level - 1];
            }

            return (level * (level - 1) / 2) * 1000;
        }

        public LinkedList<Point> Path { get; protected set; }

        public PrimaryAttributes PrimaryAttributes { get; }

        public int MaxHitPoints => 15 + PrimaryAttributes.Strength +
                                   2 * PrimaryAttributes.Endurance + Level * (PrimaryAttributes.Endurance / 2) + 2;

        public int HitPoints { get; set; }

        public int MaxCarryWeight => 25 + PrimaryAttributes.Strength * 25;

        public int SkillRate => 5 + PrimaryAttributes.Intelligence * 2;

        private double CriticalChance => 0.01 * PrimaryAttributes.Luck;

        public int Sequence => 2 * PrimaryAttributes.Perception;

        public int ActionPoints => (int)Math.Floor((PrimaryAttributes.Agility / 2d) + 5);

        public int MeleeDamage => Math.Max(1, PrimaryAttributes.Strength - 5);

        public int HealingRate => Math.Max(PrimaryAttributes.Endurance / 3, 1);

        public IReadOnlyDictionary<Skill, int> Skills { get; }

        public string Name { get; }

        public HashSet<Point> VisiblePoints { get; set; } = new HashSet<Point>();

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

        protected Character(string name, PrimaryAttributes primaryAttributes, int spriteId) : base(spriteId)
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

        private int GetSightRadius(LocationType type)
        {
            double perceptionMultiplier;
            double baseRadius = 2d;
            switch (type)
            {
                case LocationType.Island:
                    perceptionMultiplier = 4;
                    break;
                case LocationType.Cave:
                    perceptionMultiplier = 2;
                    break;
                case LocationType.Dungeon:
                    perceptionMultiplier = 3;
                    break;
                default:
                    perceptionMultiplier = 1;
                    break;
            }

            return (int)Math.Floor(baseRadius + (PrimaryAttributes.Perception * perceptionMultiplier));
        }

        public Armor EquippedArmor => Inventory.Equipped.ContainsKey(EquipSlot.Armor)
            ? Inventory.Equipped[EquipSlot.Armor] as Armor
            : Armor.NoArmor;

        public Weapon EquippedWeapon => Inventory.Equipped.ContainsKey(EquipSlot.Weapon)
            ? Inventory.Equipped[EquipSlot.Armor] as Weapon
            : Weapon.UnarmedWeapon(MeleeDamage);

        public HashSet<Point> GetVisiblePoints(Floor parent, Location location)
        {
            var algorithm = new PermissiveFov(parent.Settings.Width, parent.Settings.Height, parent.IsTransparent);
            var points = algorithm.Compute(parent.Entities.Reverse[this].X, parent.Entities.Reverse[this].Y, GetSightRadius(location.Type));

            SeenPoints.UnionWith(points);
            return points;
        }

        // We can't move to non-floor tile directly, so move to nearest neighbor and then move to entity
        protected LinkedList<Point> FindPathToEntity(Floor parent, Point target)
        {
            // find a shortest path to entity and move to it
            LinkedList<Point> shortestNeighborPath = null;

            foreach (var neighbor in parent.GetNeighbors(target))
            {
                // can't move to an unknown tile
                if (!SeenPoints.Contains(neighbor))
                {
                    continue;
                }

                var path = PathFinder.AStar(parent, parent.Entities.Reverse[this], neighbor);

                if (path == null)
                {
                    continue;
                }

                if (shortestNeighborPath == null || path.Count < shortestNeighborPath.Count)
                {
                    shortestNeighborPath = path;
                    shortestNeighborPath.AddLast(target);
                }
            }

            return shortestNeighborPath;
        }

        public ActionResult MoveTo(Floor parent, Point target)
        {
            var myPosition = parent.Entities.Reverse[this];
            if (myPosition == target)
            {
                return ActionResult.Empty;
            }

            LinkedList<Point> path = null;

            if (parent.Entities.Forward.Contains(target))
            {
                var entity = parent.Entities.Forward[target];

                if (entity is Door door)
                {
                    // if door is neighbor, just open it
                    if (parent.GetNeighbors(target, true).Any(neighbor => neighbor == myPosition))
                    {
                        door.Open();
                        return this is Player ? new SimpleResult("You open the door", LineType.General) : ActionResult.Empty;
                    }

                    path = FindPathToEntity(parent, target);
                }
                else if (entity is Character character)
                {
                    if (parent.GetNeighbors(target, true).Any(neighbor => neighbor == myPosition))
                    {
                        return Attack(character);
                    }
                }
            }
            else
            {
                path = PathFinder.AStar(parent, myPosition, target);
            }

            if (path != null)
            {
                Path = path;
            }

            return ActionResult.Empty;
        }

        public virtual ActionResult Update(GameState state)
        {
            VisiblePoints = GetVisiblePoints(state.CurrentFloor, state.PlayerLocation);
            return ActionResult.Empty;
        }

        protected ActionResult Step(Floor parent)
        {
            if (HasNextStep)
            {
                var newPos = Path.First.Value;
                Path.RemoveFirst();
                CheckForDoor(parent, newPos);

                parent.Entities.TryRemove(this);
                parent.Entities.Add(newPos, this);
            }
            return ActionResult.Empty;
        }

        public ActionResult MoveTo(Floor parent, Direction direction)
        {
            var myPosition = parent.Entities.Reverse[this];
            var newPos = NewPosition(direction, myPosition);

            if (parent.Tiles[newPos].IsPassable)
            {
                return MoveTo(parent, newPos);
            }

            return ActionResult.Empty;
        }

        private static void CheckForDoor(Floor parent, Point newPos)
        {
            foreach (var pair in parent.Entities)
            {
                if (!(pair.Value is Door door))
                {
                    continue;
                }

                if (pair.Key == newPos)
                {
                    door.Open();
                }
            }
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

        protected AttackResult Attack(Character target)
        {
            Weapon weapon = EquippedWeapon;

            int skillValue = Skills[weapon.GovernedSkill];
            double chanceToHit = AdjustHitChance(skillValue - target.ArmorClass);

            // attack on each turn will be blown at least once
            int blowsCount = Math.Max(1, (ActionPoints / weapon.ActionPointsRequired));

            var blowsList = new List<AttackResult.Blow>();

            for (int i = 0; i < blowsCount; i++)
            {
                bool isSuccessfulBlow = RandomHelper.Random.NextDouble() <= chanceToHit;
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

                bool isCriticalHit = RandomHelper.Random.NextDouble() <= CriticalChance;
                if (isCriticalHit)
                {
                    finalDamage *= 3;
                }

                target.RemoveHitPoints(finalDamage);

                blowsList.Add(new AttackResult.Blow { Damage = finalDamage, IsCriticalHit = isCriticalHit });

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

        public void AddHitPoints(int increment)
        {
            HitPoints += Math.Max(increment, 0);
            if (HitPoints > MaxHitPoints)
            {
                HitPoints = MaxHitPoints;
            }
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
            int chance = rawChance;

            if (chance < 5)
            {
                chance = 5;
            }

            if (chance > 95)
            {
                chance = 95;
            }

            return chance / 100d;
        }

        public void Regenerate()
        {
            AddHitPoints(HealingRate);
        }
    }
}