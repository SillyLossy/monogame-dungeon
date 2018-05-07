namespace Dungeon.Game.Entities.Characters
{
    public abstract class ActionResult
    {
        private class EmptyResult : ActionResult
        {

        }

        public static ActionResult Empty { get; } = new EmptyResult();
    }
}