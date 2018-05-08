using System;
using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.Entities.Characters
{
    public class Player : Character
    {
        public Func<ActionResult> PendingAction { get; set; }

        public Player(string name, PrimaryAttributes primaryAttributes, Point initialPosition, int spriteId) : base(name, primaryAttributes, initialPosition, spriteId)
        {
        }

        public override ActionResult Update(GameState state)
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
            return base.Attack(target);
        }
    }
}
