using System;
using System.Collections.Generic;
using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.Entities.Characters
{
    public class Player : Character
    {
        public Func<ActionResult> PendingAction { get; set; }

        public Player(string name, PrimaryAttributes primaryAttributes, int spriteId) : base(name, primaryAttributes, spriteId)
        {
        }

        public override ActionResult Update(GameState state)
        {
            base.Update(state);
            var result = PendingAction?.Invoke();
            if (result == null)
            {
                return Step(state.CurrentFloor);
            }
            return result;
        }
        
        public override bool IsPassable => false;
        public override bool IsTransparent => false;

        public HashSet<Point> PreviouslySeen
        {
            get
            {
                var set = new HashSet<Point>(SeenPoints);
                set.ExceptWith(VisiblePoints);
                return set;
            }
        }
    }
}
