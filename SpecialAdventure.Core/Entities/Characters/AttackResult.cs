using System.Collections.Generic;

namespace SpecialAdventure.Core.Entities.Characters
{
    public class AttackResult : ActionResult
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