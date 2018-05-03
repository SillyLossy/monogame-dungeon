using System;
using Microsoft.Xna.Framework;

namespace Dungeon.Game
{
    public class Viewport2D
    {
        private float Scale { get; set; } = 1f;
        private const float MinScale = 1f;
        private const float MaxScale = 16f;
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

        public Tuple<int, int> ToTileSize(int w, int h)
        {
            return new Tuple<int, int>((int)Math.Round(w / (Scale * TileSize)), (int)Math.Round(h / (Scale * TileSize)));
        }

        public Rectangle TranslatePoint(int x, int y)
        {
            return new Rectangle((int)Math.Ceiling((x + Left) * Scale * TileSize),
                                 (int)Math.Ceiling((y + Top) * Scale * TileSize),
                                 (int)Math.Ceiling(Scale * TileSize),
                                 (int)Math.Ceiling(Scale * TileSize));
        }

        public Point TranslateMouse(int x, int y)
        {
            return new Point(((int)Math.Floor((double) x / (Scale * TileSize)) - Left),
                             ((int)Math.Floor((double) y / (Scale * TileSize)) - Top));
        }
    }
}
