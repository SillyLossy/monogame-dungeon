using System;
using Microsoft.Xna.Framework;

namespace Dungeon.Game
{
    public class Viewport2D
    {
        private int left;
        private int top;
        public float Scale { get; set; } = 2f;
        private const float MinScale = 0.5f;
        private const float MaxScale = 8f;
        private const int TileSize = 8;

        public void UpScale()
        {
            if (Scale >= MaxScale)
            {
                return;
            }
            Scale *= 2f;
        }

        public void DownScale()
        {
            if (Scale <= MinScale)
            {
                return;
            }
            Scale /= 2f;
        }

        public int Left
        {
            get => left;
            set => left = value;
        }

        public int Top
        {
            get => top;
            set => top = value;
        }

        public Tuple<int, int> ToTileSize(int w, int h)
        {
            return new Tuple<int, int>((int) Math.Round(w / (Scale * TileSize)), (int) Math.Round(h / (Scale * TileSize)));
        } 

        public Rectangle TranslatePoint(int x, int y)
        {
            return new Rectangle((int) ((x + Left) * Scale * TileSize),
                                 (int) ((y + Top) * Scale * TileSize),
                                 (int) (Scale * TileSize), (int) (Scale * TileSize));
        }

        public Point TranslateClick(int x, int y)
        {
            return new Point(((int) Math.Floor((x / (Scale * TileSize))) - Left),
                             ((int) Math.Floor((y / (Scale * TileSize))) - Top));
        }
    }
}
