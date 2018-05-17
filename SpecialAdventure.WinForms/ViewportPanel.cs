using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SpecialAdventure.Core.Common;
using Point = SpecialAdventure.Core.Common.Point;
using Rectangle = System.Drawing.Rectangle;

namespace SpecialAdventure.WinForms
{
    public partial class ViewportPanel
    {
        public Viewport2D Viewport { get; }

        public GameState State { get; set; }

        public override Color BackColor => Color.Black;

        public Bitmap Sprites { get; set; }

        protected override bool DoubleBuffered => true;

        private readonly IReadOnlyList<Rectangle> spriteRects = PrecalculateRectangles();
        
        private static IReadOnlyList<Rectangle> PrecalculateRectangles()
        {
            var list = new List<Rectangle>();

            const int size = Viewport2D.TileSize;

            for (int i = 0; i < 1000; i++)
            {
                list.Add(new Rectangle(size * i, 0, size, size));
            }

            return list;
        }

        public event EventHandler<Point> TileClick;
        
        public ViewportPanel()
        {
            InitializeComponent();
            MouseClick += OnMouseClick;
            MouseMove += OnMouseMove;
            Paint += OnPaint;
            ResizeRedraw = true;
            Viewport = new Viewport2D(this);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private MouseEventArgs prevMouseMoveArgs;

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (prevMouseMoveArgs != null)
                {
                    var oldPos = Viewport.TranslateMouse(prevMouseMoveArgs.X, prevMouseMoveArgs.Y);
                    var newPos = Viewport.TranslateMouse(e.X, e.Y);
                    int leftDelta = oldPos.X - newPos.X;
                    int topDelta = oldPos.Y - newPos.Y;
                    if (topDelta != 0 || leftDelta != 0)
                    {
                        Viewport.Left += leftDelta;
                        Viewport.Top += topDelta;
                        Invalidate();
                    }
                }
            }

            prevMouseMoveArgs = e;
        }

        private void OnPaint(object sender, PaintEventArgs args)
        {
            var tintedBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
            if (State == null)
            {
                return;
            }
            
            args.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            args.Graphics.CompositingMode = CompositingMode.SourceOver;
            args.Graphics.SmoothingMode = SmoothingMode.None;
            args.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            args.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            var playerVisible = State.Player.VisiblePoints;
            var tiles = State.CurrentFloor.Tiles;
            foreach (var point in playerVisible)
            {
                if (Viewport.ContainsTile(point))
                {
                    Rectangle dest = Viewport.TranslatePoint(point);
                    var source = spriteRects[tiles[point].SpriteId];
                    args.Graphics.DrawImage(Sprites, dest, source, GraphicsUnit.Pixel);
                }
            }

            foreach (var pair in State.CurrentFloor.Entities)
            {
                if (playerVisible.Contains(pair.Key) && Viewport.ContainsTile(pair.Key))
                {
                    Rectangle dest = Viewport.TranslatePoint(pair.Key);
                    var source = spriteRects[pair.Value.SpriteId];
                    args.Graphics.DrawImage(Sprites, dest, source, GraphicsUnit.Pixel);
                }
            }

            var previouslySeen = State.Player.PreviouslySeen;
            foreach (var point in previouslySeen)
            {
                if (Viewport.ContainsTile(point))
                {
                    Rectangle dest = Viewport.TranslatePoint(point);
                    var source = spriteRects[tiles[point].SpriteId];
                    args.Graphics.DrawImage(Sprites, dest, source, GraphicsUnit.Pixel);
                    args.Graphics.FillRectangle(tintedBrush, dest);
                }
            }
            tintedBrush.Dispose();
        }

        private void OnMouseClick(object sender, MouseEventArgs args)
        {
            TileClick?.Invoke(this, Viewport.TranslateMouse(args.X, args.Y));
        }
    }
}
