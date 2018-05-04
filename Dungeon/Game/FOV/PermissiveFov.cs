using System;
using System.Collections.Generic;
using Dungeon.Game.Common;

namespace Dungeon.Game.FOV
{
    public class PermissiveFov
    {
        private readonly int mapWidth;
        private readonly int mapHeight;
        private readonly Func<int, int, bool> isTransparent;

        public PermissiveFov(int mapWidth, int mapHeight, Func<int, int, bool> isTransparent)
        {
            this.mapWidth = mapWidth;

            this.mapHeight = mapHeight;

            this.isTransparent = isTransparent;
        }
    
        public HashSet<Point> Compute(int startX, int startY, int radius)
        {
            var data = new FovData
            {
                IsTransparent = isTransparent,
                StartX = startX,
                StartY = startY,
                Visited = new HashSet<Point>()
            };

            // Will always see the centre.
            data.Visited.Add(new Point(startX, startY));

            // Get the dimensions of the actual field of view, making sure not to go off the map or beyond the radius.
            int minExtentX = (startX < radius ? startX : radius);
            int maxExtentX = (mapWidth - startX <= radius ? mapWidth - startX - 1 : radius);
            int minExtentY = (startY < radius ? startY : radius);
            int maxExtentY = (mapHeight - startY <= radius ? mapHeight - startY - 1 : radius);

            ComputeQuadrant(data, 1, 1, maxExtentX, maxExtentY);
            ComputeQuadrant(data, 1, -1, maxExtentX, minExtentY);
            ComputeQuadrant(data, -1, -1, minExtentX, minExtentY);
            ComputeQuadrant(data, -1, 1, minExtentX, maxExtentY);

            return data.Visited;
        }
        
        private void ComputeQuadrant(FovData data, int deltaX, int deltaY, int maxX, int maxY)
        {
            var activeViews = new List<View>();
            int i;

            var shallowLine = new Line(0, 1, maxX, 0);
            var steepLine = new Line(1, 0, 0, maxY);

            activeViews.Add(new View(shallowLine, steepLine));
            int viewIndex = 0;

            // Visit the tiles diagonally and going outwards
            //
            // .
            // .
            // 9
            // 58
            // 247
            // @136..
            int maxI = maxX + maxY;
            for (i = 1; i <= maxI && activeViews.Count != 0; ++i)
            {
                int startJ = (0 > i - maxX ? 0 : i - maxX);
                int maxJ = (i < maxY ? i : maxY);

                int j;
                for (j = startJ; j <= maxJ && viewIndex < activeViews.Count; ++j)
                {
                    VisitPoint(data, i - j, j, deltaX, deltaY, viewIndex, activeViews);
                }
            }
        }

        private void VisitPoint(FovData data, int x, int y, int deltaX, int deltaY, int viewIndex, List<View> activeViews)
        {
            var topLeft = new Point(x, y + 1);
            var bottomRight = new Point(x + 1, y); // The top left and bottom right corners of the current coordinate.

            for (;
                viewIndex < activeViews.Count &&
                activeViews[viewIndex].SteepLine.IsBelowOrCollinear(bottomRight.X, bottomRight.Y);
                ++viewIndex)
            {
                // The current coordinate is above the current view and is ignored. The steeper fields may need it though.
            }

            if (viewIndex == activeViews.Count ||
                activeViews[viewIndex].ShallowLine.IsAboveOrCollinear(topLeft.X, topLeft.Y)
            )
            {
                // Either the current coordinate is above all of the fields or it is below all of the fields.
                return;
            }

            // It is now known that the current coordinate is between the steep
            // and shallow lines of the current view.

            // The real quadrant coordinates
            int realX = x * deltaX;
            int realY = y * deltaY;


            var pt = new Point(data.StartX + realX, data.StartY + realY);

            data.Visited.Add(new Point(pt.X, pt.Y));

            if (data.IsTransparent(pt.X, pt.Y))
            {
                // The current coordinate does not block sight and therefore has no effect on the view.
                return;
            }

            // The current coordinate is an obstacle.
            bool shallowLineIsAboveBottomRight =
                activeViews[viewIndex].ShallowLine.IsAbove(bottomRight.X, bottomRight.Y);
            bool steepLineIsBelowTopLeft = activeViews[viewIndex].SteepLine.IsBelow(topLeft.X, topLeft.Y);

            if (shallowLineIsAboveBottomRight && steepLineIsBelowTopLeft)
            {
                // The obstacle is intersected by both lines in the current view. The view is completely blocked.
                activeViews.RemoveAt(viewIndex);
            }
            else if (shallowLineIsAboveBottomRight)
            {
                // The obstacle is intersected by the shallow line of the current view. The shallow line needs to be raised.
                AddShallowBump(topLeft.X, topLeft.Y, activeViews, viewIndex);
                CheckView(activeViews, viewIndex);
            }
            else if (steepLineIsBelowTopLeft)
            {
                // The obstacle is intersected by the steep line of the current view. The steep line needs to be lowered.
                AddSteepBump(bottomRight.X, bottomRight.Y, activeViews, viewIndex);
                CheckView(activeViews, viewIndex);
            }
            else
            {
                // The obstacle is completely between the two lines of the current view.
                // Split the current view into two views above and below the current coordinate.
                int shallowViewIndex = viewIndex;
                int steepViewIndex = ++viewIndex;

                activeViews.Insert(shallowViewIndex, activeViews[shallowViewIndex].Clone());
                AddSteepBump(bottomRight.X, bottomRight.Y, activeViews, shallowViewIndex);

                if (!CheckView(activeViews, shallowViewIndex))
                {
                    --steepViewIndex;
                }

                AddShallowBump(topLeft.X, topLeft.Y, activeViews, steepViewIndex);
                CheckView(activeViews, steepViewIndex);
            }
        }

        private static void AddShallowBump(int x, int y, IReadOnlyList<View> activeViews, int viewIndex)
        {
            activeViews[viewIndex].ShallowLine.SetFarPoint(x, y);
            activeViews[viewIndex].ShallowBump = new ViewBump(x, y, activeViews[viewIndex].ShallowBump);

            for (var curBump = activeViews[viewIndex].SteepBump; curBump != null; curBump = curBump.Parent)
            {
                if (activeViews[viewIndex].ShallowLine.IsAbove(curBump.X, curBump.Y))
                {
                    activeViews[viewIndex].ShallowLine.SetNearPoint(curBump.X, curBump.Y);
                }
            }
        }

        private static void AddSteepBump(int x, int y, IReadOnlyList<View> activeViews, int viewIndex)
        {
            activeViews[viewIndex].SteepLine.SetFarPoint(x, y);
            activeViews[viewIndex].SteepBump = new ViewBump(x, y, activeViews[viewIndex].SteepBump);

            for (var curBump = activeViews[viewIndex].ShallowBump; curBump != null; curBump = curBump.Parent)
            {
                if (activeViews[viewIndex].SteepLine.IsBelow(curBump.X, curBump.Y))
                {
                    activeViews[viewIndex].SteepLine.SetNearPoint(curBump.X, curBump.Y);
                }
            }
        }

        private static bool CheckView(IList<View> activeViews, int viewIndex)
        {
            /* Remove the view in activeViews at index viewIndex if either:
             * - the two lines are collinear
             * - the lines pass through either extremity
             */
            if (!activeViews[viewIndex].ShallowLine.IsLineCollinear(activeViews[viewIndex].SteepLine) ||
                (!activeViews[viewIndex].ShallowLine.IsCollinear(0, 1) &&
                 !activeViews[viewIndex].ShallowLine.IsCollinear(1, 0)))
            {
                return true;
            }

            activeViews.RemoveAt(viewIndex);
            return false;

        }


        private class FovData
        {
            public Func<int, int, bool> IsTransparent { get; set; }
            public int StartX { get; set; }
            public int StartY { get; set; }
            public HashSet<Point> Visited { get; set; }
        }
    }
}