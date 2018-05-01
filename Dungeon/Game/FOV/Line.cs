namespace Dungeon.Game.FOV
{
    internal class Line
    {
        private int nearX;
        private int nearY;
        private int farX;
        private int farY;
        private int deltaX;
        private int deltaY;

        public Line(int nearX, int nearY, int farX, int farY) {

            this.nearX = nearX;
        
            this.nearY = nearY;
        
            this.farX = farX;
        
            this.farY = farY;
        
            deltaX = this.farX - this.nearX;
        
            deltaY = this.farY - this.nearY;
        }

        public Line Clone()
        {
            return new Line(nearX, nearY, farX, farY);
        }
    
        public void SetNearPoint(int newNearX, int newNearY)
        {
            nearX = newNearX;
            nearY = newNearY;

            deltaX = farX - nearX;
            deltaY = farY - nearY;
        }
    
        public void SetFarPoint(int newFarX, int newFarY)
        {
            farX = newFarX;
            farY = newFarY;

            deltaX = farX - nearX;
            deltaY = farY - nearY;
        }
    
        public bool IsBelow(int x, int y)
        {
            return RelativeSlope(x, y) > 0;
        }


        public bool IsBelowOrCollinear(int x, int y)
        {
            return RelativeSlope(x, y) >= 0;
        }

        public bool IsAbove(int x, int y)
        {
            return RelativeSlope(x, y) < 0;
        }
    
        public bool IsAboveOrCollinear(int x, int y)
        {
            return RelativeSlope(x, y) <= 0;
        }
    
        public bool IsCollinear(int x, int y)
        {
            return RelativeSlope(x, y) == 0;
        }
    
        public bool IsLineCollinear(Line line)
        {
            return IsCollinear(line.nearX, line.nearY) && IsCollinear(line.farX, line.farY);
        }
    
        int RelativeSlope(int x, int y)
        {
            return deltaY * (farX - x) - deltaX * (farY - y);
        }
    }
}