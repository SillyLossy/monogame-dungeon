namespace Dungeon.Game.FOV
{
    internal class View
    {
        public readonly Line ShallowLine;
        public readonly Line SteepLine;
        public ViewBump ShallowBump;
        public ViewBump SteepBump;

        public View(Line shallowLine, Line steepLine)
        {
            ShallowLine = shallowLine;
            
            SteepLine = steepLine;
            
            ShallowBump = null;
            
            SteepBump = null;
        }
        
        public View Clone()
        {
            var view = new View(ShallowLine.Clone(), SteepLine.Clone())
            {
                ShallowBump = ShallowBump?.Clone(),
                SteepBump = SteepBump?.Clone()
            };

            return view;
        }
    }
}