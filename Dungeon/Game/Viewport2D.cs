using System;
using Dungeon.Game.Common;

namespace Dungeon.Game
{
    public class Viewport2D
    {
        private float Scale { get; set; } = 1f;
        private const float MinScale = 1f;
        private const float MaxScale = 8f;
        private const int TileSize = 32;
        private const float ScaleIncrement = 1.5f;

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

        public int Left { get; set; }

        public int Top { get; set; }

        public bool IsDragged { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Tuple<int, int> ToTileSize(int w, int h)
        {
            return new Tuple<int, int>((int)Math.Round(w / (Scale * TileSize)), (int)Math.Round(h / (Scale * TileSize)));
        }

        public Microsoft.Xna.Framework.Rectangle TranslatePoint(int x, int y)
        {
            return new Microsoft.Xna.Framework.Rectangle((int)Math.Ceiling((x + Left) * Scale * TileSize),
                                 (int)Math.Ceiling((y + Top) * Scale * TileSize),
                                 (int)Math.Ceiling(Scale * TileSize),
                                 (int)Math.Ceiling(Scale * TileSize));
        }

        public Point TranslateMouse(int x, int y)
        {
            return new Point(((int)Math.Floor((double) x / (Scale * TileSize)) - Left),
                             ((int)Math.Floor((double) y / (Scale * TileSize)) - Top));
        }

        public bool ContainsTile(Point point)
        {
            return (Math.Ceiling((point.X + Left) * Scale * TileSize) <= Width &&
                    Math.Ceiling((point.Y + Top) * Scale * TileSize) <= Height);
        }


        public void Center(Point point = null)
        {
            var size = ToTileSize(Width, Height);
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
                left = (-2 * point.X + size.Item1) / 2;
                top = (-2 * point.Y + size.Item2) / 2;
            }
            Left = left;
            Top = top;
        }

    }
}
