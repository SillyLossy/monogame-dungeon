using System;
using Point = SpecialAdventure.Core.Common.Point;

namespace SpecialAdventure.WinForms
{
    public class Viewport2D
    {
        public const int TileSize = 32;
        private double RealTileSize => TileSize * Scale;
        public double Scale { get; private set; } = 1f;
        public const double MinScale = 0.25f;
        public const double MaxScale = 1f;
        public const double ScaleIncrement = 2f;
        private readonly ViewportPanel form;

        public Viewport2D(ViewportPanel form)
        {
            this.form = form;
        }

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
        
        public int Width => form.Width;
        public int Height => form.Height;

        public Tuple<int, int> ToTileSize()
        {
            return new Tuple<int, int>((int)Math.Round(Width / RealTileSize),
                                       (int)Math.Round(Height / RealTileSize));
        }

        public System.Drawing.Rectangle TranslatePoint(Point point)
        {
            return new System.Drawing.Rectangle((int) Math.Ceiling((-Left + point.X) * Scale * TileSize),
                (int)Math.Ceiling((-Top + point.Y) * Scale * TileSize),
                (int)Math.Ceiling(RealTileSize),
                (int)Math.Ceiling(RealTileSize));
        }

        public Point TranslateMouse(int x, int y)
        {
            return new Point((int)Math.Floor(Left + (x / RealTileSize)),
                             ((int)Math.Floor(Top + (y / RealTileSize))));
        }

        public bool ContainsTile(Point point)
        {
            return (Math.Ceiling((-Left + point.X) * RealTileSize) <= Width &&
                    Math.Ceiling((-Top + point.Y) * RealTileSize) <= Height);
        }


        public void Center(Point point)
        {
            var size = ToTileSize();
            int left;
            int top;
            if (point == null)
            {
                // center on middle of floor
                left = (size.Item1 / 2) - (Width / 2);
                top = (size.Item2 / 2) - (Height / 2);
            }
            else
            {
                // center given point
                left = point.X - (size.Item1 / 2);
                top = point.Y + (size.Item2 / 2);
            }
            Left = left;
            Top = top;
        }

        public void Center(object sender, Point e)
        {
            Center(e);
        }
    }
}
