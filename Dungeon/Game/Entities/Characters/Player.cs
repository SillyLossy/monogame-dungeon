using System;
using Dungeon.Game.Common;

namespace Dungeon.Game.Entities.Characters
{
    public class Player : Character
    {
        public Func<ActionResult> PendingAction { get; set; }

        public Player(string name, PrimaryAttributes primaryAttributes, Point initialPosition, int spriteId) : base(name, primaryAttributes, initialPosition, spriteId)
        {
        }

        public override ActionResult Update(DungeonGameState state)
        {
            var result = PendingAction?.Invoke();
            if (result == null)
            {
                return Step(state.CurrentFloor);
            }
            return result;
        }

        protected override AttackResult Attack(Character target)
        {
            var result = base.Attack(target);
            DungeonGame.Log.LogPlayerAttack(result);
            return result;
        }
    }
}
