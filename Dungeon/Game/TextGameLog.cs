using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dungeon.Game.Entities;

namespace Dungeon.Game
{
    public class TextGameLog
    {
        private readonly LinkedList<string> lines = new LinkedList<string>();

        public void PushLine(string line)
        {
            lines.AddLast(line);
        }

        public IEnumerable<string> GetLastLines(int count)
        {
            return lines.Skip(Math.Max(0, lines.Count - count));
        }

        public void LogAttack(AttackResult result)
        {
            lines.AddLast(result.Attacker is Player ? DescribePlayerAttack(result) : DescribeMonsterAttack(result));
        }

        private static string DescribeMonsterAttack(AttackResult result)
        {
            var sb = new StringBuilder(40);

            foreach (var blow in result.Blows)
            {
                if (blow.IsMiss)
                {
                    sb.AppendFormat("{0} misses. ", result.Attacker.Name);
                }
                else
                {
                    sb.AppendFormat("{0} have dealt {1} damage to you. ", result.Attacker.Name, blow.Damage);
                }
            }

            if (result.Target.IsDead)
            {
                sb.AppendFormat("{0} deals a killing blow to you.", result.Attacker.Name);
            }

            return sb.ToString();
        }

        private static string DescribePlayerAttack(AttackResult result)
        {
            var sb = new StringBuilder(40);

            foreach (var blow in result.Blows)
            {
                if (blow.IsMiss)
                {
                    sb.AppendFormat("You have missed the {0} . ", result.Target.Name);
                }
                else
                {
                    sb.AppendFormat("You have dealt {0} damage to {1}. ", blow.Damage, result.Target.Name);
                }
            }

            if (result.Target.IsDead)
            {
                sb.AppendFormat("{0} dies. ", result.Target.Name);
            }

            return sb.ToString();
        }
    }
}
