using System;
using System.Collections.Generic;
using System.Text;

namespace Dungeon.Game
{
    class TextGameLog
    {
        private const int MaxLineLenght = 5;
        private const int MaxLines = 5;

        private readonly LinkedList<string> lines = new LinkedList<string>();

        private void PushShortLine(string line)
        {
            if (lines.Count + 1 > MaxLines)
            {
                lines.RemoveFirst();
            }
            lines.AddLast(line);
        }

        public void PushLine(string line)
        {
            if (line.Length <= MaxLineLenght)
            {
                PushShortLine(line);
            }
            else
            {
                PushLongLine(line);
            }

        }

        private void PushLongLine(string line)
        {
            var sb = new StringBuilder(line);

            for (int i = 0; i < line.Length / MaxLineLenght + 1; i++)
            {
                sb.Insert(i * MaxLineLenght, value: '\n');
            }

            foreach (string substring in sb.ToString().Split('\n'))
            {
                PushShortLine(substring);
            }
        }
    }
}
