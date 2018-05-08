using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities;
using SpecialAdventure.Core.Entities.Characters;
using SpecialAdventure.Core.Log;
using SpecialAdventure.Core.World.Tiles;
using Point = SpecialAdventure.Core.Common.Point;

namespace Dungeon.Game
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class DungeonGame : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private const int UpdatesPerSecond = 15;
        private const double TimeBetweenUpdates = 1000d / UpdatesPerSecond;
        private const string SaveFilePath = "Save.dgn";
        private MouseState? prevMouseState;
        private MouseState? prevViewportMouseState;
        private Action inputAction;
        private Texture2D Sprites;
        private TimeSpan lastUpdate;
        private KeyboardState? prevKeyboardState;
        private readonly GameState gameState;
        private readonly Viewport2D viewport;
        private static readonly Color SeenTileColor = new Color(100, 100, 100, 100);
        private SpriteFont defaultFont;
        private SpriteFont smallFont;
        public static Random Random { get; } = new Random();

        private IReadOnlyList<Microsoft.Xna.Framework.Rectangle> spriteRects = PrecalculateRectangles();

        private static IReadOnlyList<Microsoft.Xna.Framework.Rectangle> PrecalculateRectangles()
        {
            var list = new List<Microsoft.Xna.Framework.Rectangle>();

            const int size = Viewport2D.TileSize;

            for (int i = 0; i < 1000; i++)
            {
                list.Add(new Microsoft.Xna.Framework.Rectangle(size * i, 0, size, size));
            }

            return list;
        }


        private static GameState LoadGame()
        {
            try
            {
                // return JsonConvert.DeserializeObject<GameState>(File.ReadAllText(SaveFilePath), SerializerSettings);
            }
            catch (Exception)
            {
            }
            return new GameState().NewGame();
        }

        private static void SaveGame(GameState state)
        {
            try
            {
                // File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(state, SerializerSettings));
            }
            catch (Exception)
            {
                // panic!
            }
        }

        public DungeonGame()
        {
            gameState = LoadGame();

            graphics = new GraphicsDeviceManager(this)
            {
                //PreferredBackBufferWidth = 300,
                //PreferredBackBufferHeight = 200
            };
            Content.RootDirectory = "Content";
            viewport = new Viewport2D();
            SizeChanged(Window, EventArgs.Empty);
            viewport.Center(gameState.Player.Position);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += SizeChanged;
            base.Initialize();
        }

        private void SizeChanged(object sender, EventArgs e)
        {
            var window = (GameWindow) sender;
            graphics.PreferredBackBufferHeight = window.ClientBounds.Height;
            graphics.PreferredBackBufferWidth = window.ClientBounds.Width;
            viewport.Width = (int) (graphics.PreferredBackBufferWidth );
            viewport.Height = (int) (graphics.PreferredBackBufferHeight );
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            defaultFont = Content.Load<SpriteFont>("DefaultFont");
            smallFont = Content.Load<SpriteFont>("SmallFont");

            Sprites = Content.Load<Texture2D>("Sprites");
        }

        private Texture2D CreateTexture(Color color, int width, int height)
        {
            var result = new Texture2D(GraphicsDevice, width, height);
            result.SetData(Enumerable.Repeat(color, width * height).ToArray());
            return result;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            SaveGame(gameState);
            Sprites.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if ((gameTime.TotalGameTime - lastUpdate).TotalMilliseconds < TimeBetweenUpdates)
            {
                // This will record action in between the processed time frames
                HandleMouse();
                HandleKeyboard();
                HandleViewport();
                return;
            }

            gameState.Update(inputAction);

            lastUpdate = gameTime.TotalGameTime;
            inputAction = null;
            base.Update(gameTime);
        }

        private void HandleViewport()
        {
            MouseState mouseState = Mouse.GetState(Window);
            var mousePos = viewport.TranslateMouse(mouseState.X, mouseState.Y);
            if (prevViewportMouseState != null)
            {
                MouseState prevState = prevViewportMouseState.Value;
                if (prevState.ScrollWheelValue < mouseState.ScrollWheelValue)
                {
                    viewport.UpScale();
                    viewport.Center(gameState.Player.Position);
                }
                else if (prevState.ScrollWheelValue > mouseState.ScrollWheelValue)
                {
                    viewport.DownScale();
                    viewport.Center(gameState.Player.Position);
                }

                if (prevState.LeftButton == ButtonState.Pressed &&
                    mouseState.LeftButton == ButtonState.Pressed)
                {
                    var oldPos = viewport.TranslateMouse(prevState.X, prevState.Y);
                    var newPos = mousePos;
                    int leftDelta = oldPos.X - newPos.X;
                    int topDelta = oldPos.Y - newPos.Y;
                    if (topDelta != 0 || leftDelta != 0)
                    {
                        viewport.Left += leftDelta;
                        viewport.Top += topDelta;
                        viewport.IsDragged = true;
                    }
                }
                else
                {
                    // prevent mouse action on release
                    viewport.IsDragged = false;
                    prevMouseState = null;
                }
            }

            prevViewportMouseState = mouseState;
        }

        private void HandleMouse()
        {
            if (viewport.IsDragged)
            {
                return;
            }

            var mouseState = Mouse.GetState(Window);

            if (prevMouseState?.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
            {
                inputAction = () => MovePlayerByClick(mouseState);
            }

            prevMouseState = mouseState;
        }

        private void HandleKeyboard()
        {
            var keyboardState = Keyboard.GetState();
            Direction? direction = null;

            // prevent input if key is being hold
            if (prevKeyboardState.HasValue && keyboardState == prevKeyboardState)
            {
                return;
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                direction = Direction.West;
            }
            else if (keyboardState.IsKeyDown(Keys.Right))
            {
                direction = (Direction.East);
            }
            else if (keyboardState.IsKeyDown(Keys.Up))
            {
                direction = (Direction.North);
            }
            else if (keyboardState.IsKeyDown(Keys.Down))
            {
                direction = (Direction.South);
            }

            if (direction.HasValue)
            {
                inputAction = (() => gameState.MovePlayer(direction.Value));
            }

            if (keyboardState.IsKeyDown(Keys.PageDown))
            {
                inputAction = (() => gameState.Descend());
                viewport.Center(gameState.Player.Position);
            }

            if (keyboardState.IsKeyDown(Keys.PageUp))
            {
                inputAction = (() => gameState.Ascend());
                viewport.Center(gameState.Player.Position);
            }

            prevKeyboardState = keyboardState;
        }


        private void MovePlayerByClick(MouseState mouseState)
        {
            var bufferRect = new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // prevent processing mouse outside the window
            if (bufferRect.Contains(mouseState.X, mouseState.Y))
            {
                var point = viewport.TranslateMouse(mouseState.X, mouseState.Y);
                gameState.MovePlayer(point);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Tuple<int, int> size = viewport.ToTileSize();
            int x = viewport.Left, y = viewport.Top, w = size.Item1, h = size.Item2;
            Dictionary<Point, Tile> level = gameState.GetTiles(new SpecialAdventure.Core.Common.Rectangle(x, y, w, h));

            GraphicsDevice.Clear(Color.Black);
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            foreach (KeyValuePair<Point, Tile> pair in level)
            {
                var target = viewport.TranslatePoint(pair.Key);
                spriteBatch.Draw(Sprites, target, spriteRects[pair.Value.SpriteId], Color.White);
            }

            //var visible = gameState.Player.GetVisiblePoints(gameState.CurrentFloor);

            // DrawVisible(visible, level);

            //DrawDoors(visible, Color.White);

            //DrawSeenPoints(visible, level);

            //DrawDoors(gameState.Player.SeenPoints, SeenTileColor);

            //DrawPath();

            //DrawCharacters(visible);

            //DrawLog();

            //DrawStatus();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawVisible(HashSet<Point> visible, Tile[,] level)
        {
            foreach (var point in visible)
            {
                var rect = viewport.TranslatePoint(point);
                if (viewport.ContainsTile(point))
                {
                    //spriteBatch.Draw(Sprites[level[point.X, point.Y]], rect,  Color.White);
                }
            }
        }

        private void DrawSeenPoints(HashSet<Point> visible, Tile[,] level)
        {
            foreach (var point in gameState.Player.SeenPoints)
            {
                var rect = viewport.TranslatePoint(point);
                if (!visible.Contains(point) && viewport.ContainsTile(point))
                {
                    //spriteBatch.Draw(Sprites[level[point.X, point.Y]], rect, SeenTileColor);
                }
            }
        }

        private void DrawPath()
        {
            // Paint path for player
            // TODO: Remove this after debugging
            if (gameState.Player.Path != null)
            {
                foreach (var point in gameState.Player.Path)
                {
                    if (viewport.ContainsTile(point))
                    {
                        //spriteBatch.Draw(Sprites[Game.Sprites.Path], viewport.TranslatePoint(point.X, point.Y), Color.White);
                    }
                }
            }
        }

        private void DrawCharacters(HashSet<Point> visible)
        {
            // Draw characters
            foreach (var character in gameState.CurrentFloor.Characters)
            {
                if (viewport.ContainsTile(character.Position) && visible.Contains(character.Position))
                {
                    //spriteBatch.Draw(Sprites[character.TextureKey],
                    //    viewport.TranslatePoint(character.Position.X, character.Position.Y), Color.White);
                }
            }
        }
        
        private void DrawLog()
        {
            const int xMargin = 3, yMargin = 2;

            var logRect = new Microsoft.Xna.Framework.Rectangle(0, viewport.Height, viewport.Width, Window.ClientBounds.Height - viewport.Height);
            //spriteBatch.Draw(Sprites[Game.Sprites.Target], logRect, Color.Black);

            int lineCount = (int) Math.Floor((logRect.Height - yMargin * 2) / (double)(smallFont.LineSpacing));
            
            int topOffsetIndex = 0;
            foreach (var line in gameState.Log.GetLastLines(lineCount))
            {
                spriteBatch.DrawString(smallFont, line.Line, new Vector2(logRect.X + xMargin, (logRect.Y + yMargin) + smallFont.LineSpacing * topOffsetIndex++), LogColors[line.Type]);
            }
        }

        private static readonly IReadOnlyDictionary<LineType, Color> LogColors = new Dictionary<LineType, Color>
        {
            { LineType.General, Color.White }
        };

        private void DrawStatus()
        {
            const int xMargin = 3, yMargin = 2;

            var statusRect = new Microsoft.Xna.Framework.Rectangle(viewport.Width, 0, Window.ClientBounds.Width - viewport.Width, Window.ClientBounds.Height);
            //spriteBatch.Draw(Sprites[Game.Sprites.Target], statusRect, Color.Black);

            var player = gameState.Player;
            
            int expTillNext = Character.GetExperienceCap(player.Level + 1) - player.Experience;
            var lines = new List<LogLine>
            {
                new LogLine(player.Name),
                new LogLine(string.Format("Level {0} ({1} exp. till next)", player.Level, expTillNext)),
                new LogLine(string.Format("Health: {0}/{1}", player.HitPoints, player.MaxHitPoints))
            };


            int topOffsetIndex = 0;
            foreach (var line in lines)
            {
                spriteBatch.DrawString(smallFont, line.Line, new Vector2(statusRect.X + xMargin, (statusRect.Y + yMargin) + smallFont.LineSpacing * topOffsetIndex++), LogColors[line.Type]);
            }
        }

        private void DrawDoors(HashSet<Point> visiblePoints, Color color)
        {
            foreach (var door in gameState.CurrentFloor.Doors)
            {
                if (viewport.ContainsTile(door.Position) && visiblePoints.Contains(door.Position))
                {
                    //Texture2D texture = Sprites[door.TextureKey];
                    //spriteBatch.Draw(texture, viewport.TranslatePoint(door.Position.X, door.Position.Y), color);
                }
            }
        }
    }
}
