using System;
using System.Collections.Generic;
using System.Linq;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.World;

namespace SpecialAdventure.Core.Entities.Characters
{
    public class Monster : Character
    {
        public enum State
        {
            Roaming, Waiting, Enraged
        }

        public State CurrentState { get; private set; } = State.Waiting;

        private int WaitTime { get; set; }

        private int Cooldown { get; set; }

        private Character RivalCharacter { get; set; }
        
        private static T Choise<T>(params T[] variants)
        {
            return variants[RandomHelper.Random.Next(0, variants.Length)];
        }

        public Monster(string name, PrimaryAttributes primaryAttributes, int spriteId) : base(name, primaryAttributes, spriteId)
        {

        }

        public override ActionResult Update(GameState state)
        {
            base.Update(state);
            CheckForNearbyRivals(state);
            switch (CurrentState)
            {
                case State.Waiting:
                    Wait(state.CurrentFloor);
                    break;
                case State.Enraged:
                    return TryAttack(state);
                case State.Roaming:
                    Roam(state.CurrentFloor);
                    break;
                default:
                    throw new ArgumentException(nameof(state));
            }

            return ActionResult.Empty;
        }

        private void CheckForNearbyRivals(GameState state)
        {
            const int cooldown = 20;
            foreach (var character in state.CurrentFloor.Entities)
            {
                // TODO: Make monsters attack not only players
                if (character.Value is Player player && VisiblePoints.Contains(character.Key))
                {
                    RivalCharacter = player;
                    CurrentState = State.Enraged;
                    Cooldown = cooldown;
                    break;
                }
            }
        }

        private void Roam(Floor parent)
        {
            if (HasNextStep)
            {
                Step(parent);
            }
            else
            {
                SetPeacefulState(parent);
            }
        }

        private ActionResult TryAttack(GameState state)
        {
            var myPosition = state.CurrentFloor.Entities.Reverse[this];
            bool noRival = RivalCharacter == null || !state.CurrentFloor.Entities.Reverse.Contains(RivalCharacter);
            if (noRival || Cooldown <= 0)
            {
                SetPeacefulState(state.CurrentFloor);
                return ActionResult.Empty;
            }

            var rivalPosition = state.CurrentFloor.Entities.Reverse[RivalCharacter];

            if (state.CurrentFloor.GetNeighbors(myPosition, true).ToList().Contains(rivalPosition))
            {
                return Attack(RivalCharacter);
            }

            var path = FindPathToEntity(state.CurrentFloor, rivalPosition);
            if (path == null)
            {
                Cooldown--;
            }
            else
            {
                Path = path;
                Step(state.CurrentFloor);
            }

            return ActionResult.Empty;
        }

        private void Wait(Floor parent)
        {
            WaitTime--;
            if (WaitTime <= 0)
            {
                SetPeacefulState(parent);
            }
        }

        private void SetPeacefulState(Floor parent)
        {
            State newState = Choise(State.Roaming, State.Waiting);
            if (newState == State.Waiting)
            {
                SetWaitState();
            }
            else
            {
                SetRoamState(parent);
            }
        }

        private void SetRoamState(Floor parent)
        {
            var myPosition = parent.Entities.Reverse[this];
            CurrentState = State.Roaming;
            var visible = VisiblePoints.ToArray();
            if (visible.Length != 0)
            {
                var roamPoint = visible[RandomHelper.Random.Next(0, visible.Length)];
                var path = PathFinder.AStar(parent, myPosition, roamPoint);

                if (path != null)
                {
                    Path = path;
                    return;
                }
            }

            RandomMoveDirection.Shuffle();
            foreach (var direction in RandomMoveDirection)
            {
                if (parent.IsPointPassable(NewPosition(direction, myPosition)))
                {
                    MoveTo(parent, direction);
                    return;
                }
            }
            SetWaitState();
        }

        private static readonly List<Direction> RandomMoveDirection = new List<Direction>
        {
            Direction.East,
            Direction.West,
            Direction.North,
            Direction.South,
            Direction.NorthEast,
            Direction.NorthWest,
            Direction.SouthEast,
            Direction.SouthWest
        };

        private void SetWaitState()
        {
            Path?.Clear();
            WaitTime = RandomHelper.Random.Next(1, 10);
            CurrentState = State.Waiting;
        }

        public override bool IsPassable => IsDead;
        public override bool IsTransparent => IsDead;
    }
}
