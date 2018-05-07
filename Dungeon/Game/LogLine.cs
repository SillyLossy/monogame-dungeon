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
}