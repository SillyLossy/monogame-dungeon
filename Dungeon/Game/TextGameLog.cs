using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dungeon.Game.Entities;
using Microsoft.Xna.Framework;

namespace Dungeon.Game
{
    public class LogLine
    {
        public string Line { get; private set; }
        public Color Color { get; private set; }

        public LogLine(string line, Color color)
        {
            Line = line;
            Color = color;
        }

        public LogLine(string line)
        {
            Line = line;
            Color = Color.White;
        }
    }

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

        public void LogAttack(AttackResult result)
        {
            if (result.Attacker is Player)
            {
                DescribePlayerAttack(result);
            }
            else
            {
                DescribeMonsterAttack(result);
            }
        }

        private void DescribeMonsterAttack(AttackResult result)
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

        private void DescribePlayerAttack(AttackResult result)
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
    }
}
