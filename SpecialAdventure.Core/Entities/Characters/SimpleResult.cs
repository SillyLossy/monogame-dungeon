using SpecialAdventure.Core.Log;

namespace SpecialAdventure.Core.Entities.Characters
{
    public class SimpleResult : ActionResult
    {
        public string Text { get; }
        public LineType Type { get; }

        public SimpleResult(string text)
        {
            Text = text;
            Type = LineType.General;
        }

        public SimpleResult(string text, LineType lineType)
        {
            Text = text;
            Type = lineType;
        }
    }
}
