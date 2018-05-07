namespace Dungeon.Game.Entities.Characters
{
    public class SimpleResult : ActionResult
    {
        public string Text { get; set; }

        public SimpleResult(string text)
        {
            Text = text;
        }

        public SimpleResult()
        {

        }
    }
}
