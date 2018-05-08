namespace SpecialAdventure.Core.FOV
{
    internal class ViewBump
    {
        public readonly ViewBump Parent;
        public readonly int X;
        public readonly int Y;

        public ViewBump(int x, int y, ViewBump parent) {
            X = x;
            
            Y = y;
            
            Parent = parent;
        }
        
        public ViewBump Clone ()
        {
            return new ViewBump(X, Y, Parent?.Clone());
        }
    }
}