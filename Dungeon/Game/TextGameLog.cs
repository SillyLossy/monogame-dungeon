using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Entities.Characters;
using Microsoft.Xna.Framework;

namespace Dungeon.Game
{
    public class TextGameLog
    {

        private readonly LinkedList<LogLine> lines = new LinkedList<LogLine>();

        public void PushLine(LogLine line)
        {
            lines.AddLast(line);
        }

        public IEnumerable<LogLine> GetLastLines(int count)
        {
            return lines.Skip(Math.Max(0, lines.Count - count));
        }
        
        public void LogMonsterAttack(AttackResult result)
        {
            foreach (var blow in result.Blows)
            {
                if (blow.IsMiss)
                {
                    lines.AddLast(new LogLine(string.Format("{0} misses.", result.Attacker.Name), Color.WhiteSmoke));
                }
                else
                {
                    lines.AddLast(new LogLine(string.Format("The {0} hits you.", result.Attacker.Name)));
                }
            }

            if (result.Target.IsDead)
            {
                lines.AddLast(new LogLine(string.Format("{0} kills you!", result.Attacker.Name), Color.Red));
            }
        }

        public void LogPlayerAttack(AttackResult result)
        {
            foreach (var blow in result.Blows)
            {
                if (blow.IsMiss)
                {
                    lines.AddLast(new LogLine(string.Format("You miss the {0}. ", result.Target.Name), Color.WhiteSmoke));
                }
                else
                {
                    lines.AddLast(new LogLine(string.Format("You hit the {0}.", result.Target.Name)));
                }
            }

            if (result.Target.IsDead)
            {
                lines.AddLast(new LogLine(string.Format("You kill the {0}!", result.Target.Name), Color.Red));
            }
            
        }

        public void LogActionResult(ActionResult result)
        {
            throw new NotImplementedException();
        }
    }
}
