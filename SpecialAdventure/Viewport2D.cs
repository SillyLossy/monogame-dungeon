using System;
using SpecialAdventure.Core.Common;

namespace SpecialAdventure
{
    public class Viewport2D
    {
        public const int TileSize = 32;
        public double RealTileSize => TileSize * Scale;
        private double Scale { get; set; } = 1f;
        private const double MinScale = 0.5f;
        private const double MaxScale = 8f;
        private const double ScaleIncrement = 1.5f;

        public void UpScale()
        {
            if (Scale >= MaxScale)
            {
                return;
            }
            Scale *= ScaleIncrement;
        }

        public void DownScale()
        {
            if (Scale <= MinScale)
            {
                return;
            }
            Scale /= ScaleIncrement;
        }

        public int Left { get; set; } = 200;

        public int Top { get; set; } = 200;

        public bool IsDragged { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Tuple<int, int> ToTileSize()
        {
            return new Tuple<int, int>((int)Math.Round(Width / RealTileSize),
                                       (int)Math.Round(Height / RealTileSize));
        }

        public Microsoft.Xna.Framework.Rectangle TranslatePoint(Point point)
        {
            return new Microsoft.Xna.Framework.Rectangle((int)Math.Ceiling((-Left + point.X) * Scale * TileSize),
                (int)Math.Ceiling((-Top + point.Y) * Scale * TileSize),
                (int)Math.Ceiling(Scale * TileSize),
                (int)Math.Ceiling(Scale * TileSize));
        }

        public Point TranslateMouse(int x, int y)
        {
            return new Point(((int)Math.Floor(x / RealTileSize) - Left),
                             ((int)Math.Floor(y / RealTileSize) - Top));
        }

        public bool ContainsTile(Point point)
        {
            return (Math.Ceiling((point.X + Left) * Scale * TileSize) <= Width &&
                    Math.Ceiling((point.Y + Top) * Scale * TileSize) <= Height);
        }


        public void Center(Point point)
        {
            //var size = ToTileSize();
            //int left;
            //int top;
            //if (point == null)
            //{
            //    // center on middle of floor
            //    left = (size.Item1 / 2) - (Width / 2);
            //    top = (size.Item2 / 2) - (Height / 2);
            //}
            //else
            //{
            //    // center given point
            //    left = (-2 * point.X + size.Item1) / 2;
            //    top = (-2 * point.Y + size.Item2) / 2;
            //}
            //Left = left;
            //Top = top;
        }

    }
}
