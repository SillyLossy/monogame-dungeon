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

        public Monster(string name, PrimaryAttributes primaryAttributes, Point initialPosition, int spriteId) : base(name, primaryAttributes, initialPosition, spriteId)
        {

        }

        public override ActionResult Update(GameState state)
        {
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
            var visible = GetVisiblePoints(state.CurrentFloor);
            foreach (var character in state.CurrentFloor.Characters)
            {
                // TODO: Make monsters attack not only players
                if (character is Player && visible.Contains(character.Position))
                {
                    RivalCharacter = character;
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
            if (RivalCharacter == null || Cooldown <= 0)
            {
                SetPeacefulState(state.CurrentFloor);
                return ActionResult.Empty;
            }

            if (state.CurrentFloor.GetNeighbors(Position, true).ToList().Contains(RivalCharacter.Position))
            {
                return Attack(RivalCharacter);
            }

            var path = FindPathToEntity(state.CurrentFloor, RivalCharacter.Position);
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
            CurrentState = State.Roaming;
            var visible = GetVisiblePoints(parent).ToArray();
            if (visible.Length != 0)
            {
                var roamPoint = visible[RandomHelper.Random.Next(0, visible.Length)];
                var path = PathFinder.AStar(parent, Position, roamPoint);

                if (path != null)
                {
                    Path = path;
                    return;
                }
            }

            RandomMoveDirection.Shuffle();
            foreach (var direction in RandomMoveDirection)
            {
                if (parent.CanEntityMove(this, direction))
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
    }
}
