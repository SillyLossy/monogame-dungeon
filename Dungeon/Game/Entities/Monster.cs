using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Common;
using Dungeon.Game.Levels;

namespace Dungeon.Game.Entities
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
            return variants[DungeonGame.Random.Next(0, variants.Length)];
        }

        public Monster(string name, string textureKey, PrimaryAttributes primaryAttributes, Point initialPosition) : base(name, textureKey, primaryAttributes, initialPosition)
        {

        }

        public override void Update(DungeonGameState state)
        {
            CheckForNearbyRivals(state);
            switch (CurrentState)
            {
                case State.Waiting:
                    Wait(state.CurrentFloor);
                    break;
                case State.Enraged:
                    TryAttack(state);
                    break;
                case State.Roaming:
                    Roam(state.CurrentFloor);
                    break;
            }
        }

        private void CheckForNearbyRivals(DungeonGameState state)
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

        private void Roam(DungeonFloor parent)
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

        private void TryAttack(DungeonGameState state)
        {
            if (RivalCharacter == null || Cooldown <= 0)
            {
                SetPeacefulState(state.CurrentFloor);
                return;
            }

            if (state.CurrentFloor.GetNeighbors(Position, true).ToList().Contains(RivalCharacter.Position))
            {
                var result = Attack(RivalCharacter);
                if (result.Target is Player)
                {
                    DungeonGame.Log.LogAttack(result);
                }
            }
            else
            {
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
            }
        }

        private void Wait(DungeonFloor parent)
        {
            WaitTime--;
            if (WaitTime <= 0)
            {
                SetPeacefulState(parent);
            }
        }

        private void SetPeacefulState(DungeonFloor parent)
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

        private void SetRoamState(DungeonFloor parent)
        {
            CurrentState = State.Roaming;
            var visible = GetVisiblePoints(parent).ToArray();
            if (visible.Length != 0)
            {
                var roamPoint = visible[DungeonGame.Random.Next(0, visible.Length)];
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
            WaitTime = DungeonGame.Random.Next(1, 10);
            CurrentState = State.Waiting;
        }
    }
}
