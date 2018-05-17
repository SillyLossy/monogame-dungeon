using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Characters;
using SpecialAdventure.Core.Log;
using Point = SpecialAdventure.Core.Common.Point;

namespace SpecialAdventure.WinForms
{
    public partial class GameForm : Form
    {
        private GameState gameState;

        private const int UpdatesPerSecond = 10;

        private Timer timer;

        private Func<ActionResult> inputAction;

        private static readonly IReadOnlyDictionary<LineType, Color> LogColors = new Dictionary<LineType, Color>
        {
            [LineType.General] = Color.Black,
            [LineType.Death] = Color.Red,
            [LineType.Hit] = Color.ForestGreen,
            [LineType.Info] = Color.DarkBlue,
            [LineType.Miss] = Color.DimGray
        };

        public GameForm()
        {
            InitializeComponent();

            KeyUp += OnKeyUp;
            MouseWheel += OnMouseWheel;
            Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            gameState = new GameState();
            gameState.NewGame();
            gamePanel.State = gameState;
            timer = new Timer { Interval = 1000 / UpdatesPerSecond };
            timer.Tick += OnTick;
            timer.Start();
            gamePanel.TileClick += OnTileClick;
            gamePanel.Sprites = new Bitmap(Properties.Resources.Sprites);
            gamePanel.Viewport.Center(gameState.CurrentFloor.Entities.Reverse[gameState.Player]);
            gameState.PlayerWarped += gamePanel.Viewport.Center;
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (gamePanel.Bounds.Contains(e.X, e.Y))
            {
                if (e.Delta == 0)
                {
                    return;
                }

                if (e.Delta > 0)
                {
                    gamePanel.Viewport.UpScale();
                }
                else
                {
                    gamePanel.Viewport.DownScale();
                }

                gamePanel.Invalidate();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            Direction? direction = null;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    direction = Direction.West;
                    break;
                case Keys.Right:
                    direction = (Direction.East);
                    break;
                case Keys.Up:
                    direction = (Direction.North);
                    break;
                case Keys.Down:
                    direction = (Direction.South);
                    break;
            }

            if (direction.HasValue)
            {
                inputAction = (() => gameState.MovePlayer(direction.Value));
            }

            switch (e.KeyCode)
            {
                case Keys.PageDown:
                    inputAction = (() => gameState.Descend());
                    break;
                case Keys.PageUp:
                    inputAction = (() => gameState.Ascend());
                    break;
            }
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            bool updated = gameState.Update(inputAction);
            inputAction = null;
            if (updated)
            {
                gamePanel.Invalidate();
            }

            gameLogList.BeginUpdate();
            foreach (var line in gameState.Log.GetPendingLines())
            {
                var item = gameLogList.Items.Add(line.Text);
                item.ForeColor = LogColors[line.Type];
            }

            if (gameLogList.Items.Count > 0)
            {
                gameLogList.Items[gameLogList.Items.Count - 1].EnsureVisible();
            }
            gameLogList.EndUpdate();
        }

        private void OnTileClick(object sender, Point point)
        {
            inputAction = () => gameState.MovePlayer(point);
        }


        private void CharacterButton_Click(object sender, EventArgs e)
        {

        }

        private void InventoryButton_Click(object sender, EventArgs e)
        {

        }
    }
}
