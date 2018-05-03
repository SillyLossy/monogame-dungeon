using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

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
        private Dictionary<object, Texture2D> Textures;
        private TimeSpan lastUpdate;
        private KeyboardState? prevKeyboardState;
        private readonly DungeonGameState gameState;
        private readonly Viewport2D viewport;
        private static readonly Color SeenTileColor = new Color(100, 100, 100, 100);
        private static SpriteFont font;
        public static TextGameLog Log { get; } = new TextGameLog();
        public static Random Random { get; } = new Random();

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public DungeonGame()
        {
            try
            {
                gameState = File.Exists(SaveFilePath)
                    ? JsonConvert.DeserializeObject<DungeonGameState>(File.ReadAllText(SaveFilePath), SerializerSettings)
                    : new DungeonGameState().NewGame();
            }
            catch (Exception)
            {
                gameState = new DungeonGameState().NewGame();
            }

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.9),
                PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.9)
            };
            Content.RootDirectory = "Content";
            viewport = new Viewport2D();
            CenterViewport(gameState.Player.Position);
        }

        private void CenterViewport(Point? point = null)
        {
            var size = viewport.ToTileSize(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            int left;
            int top;
            if (point == null)
            {
                // center on middle of floor
                left = (size.Item1 / 2) - (gameState.CurrentFloor.Settings.Width / 2);
                top = (size.Item2 / 2) - (gameState.CurrentFloor.Settings.Height / 2);
            }
            else
            {
                // center given point
                left = (size.Item1 / 2) - (point.Value.X);
                top = (size.Item2 / 2) - (point.Value.Y);
            }
            viewport.Left = left;
            viewport.Top = top;
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
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("DefaultFont");

            Textures = new Dictionary<object, Texture2D>
            {
                [DungeonTile.Floor] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.Floor)),
                [DungeonTile.Stone] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.Stone)),
                [DungeonTile.Wall] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.Wall)),
                [TextureKey.Player] = Content.Load<Texture2D>(TextureKey.Player),
                [TextureKey.DoorOpen] = Content.Load<Texture2D>(TextureKey.DoorOpen),
                [TextureKey.DoorClosed] = Content.Load<Texture2D>(TextureKey.DoorClosed),
                [DungeonTile.LadderUp] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.LadderUp)),
                [DungeonTile.LadderDown] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.LadderDown)),
                [TextureKey.Path] = CreateTexture(new Color(0, 255, 0, 25), 8, 8),
                [TextureKey.Target] = CreateTexture(Color.Green, 8, 8),
                [DungeonTile.Test] = CreateTexture(Color.IndianRed, 8, 8),
                ["Textures/Goblin"] = Content.Load<Texture2D>("Textures/Goblin"),
                ["Textures/Tortoise"] = Content.Load<Texture2D>("Textures/Tortoise")
            };
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
            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(gameState, SerializerSettings));
            foreach (var pair in Textures)
            {
                pair.Value.Dispose();
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if ((gameTime.TotalGameTime - lastUpdate).TotalMilliseconds < TimeBetweenUpdates)
            {
                // This will record action in between the processed time frames
                HandleMouse();
                HandleKeyboard();
                HandleViewport();
                return;
            }

            // Only do update if we have a pending action or the player is already moving.
            if (inputAction != null || gameState.Player.HasNextStep)
            {
                inputAction?.Invoke();
                gameState.Step();
            }

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
                    CenterViewport(gameState.Player.Position);
                }
                else if (prevState.ScrollWheelValue > mouseState.ScrollWheelValue)
                {
                    viewport.DownScale();
                    CenterViewport(gameState.Player.Position);
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
                        viewport.Left -= leftDelta;
                        viewport.Top -= topDelta;
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
                CenterViewport(gameState.Player.Position);
            }

            if (keyboardState.IsKeyDown(Keys.PageUp))
            {
                inputAction = (() => gameState.Ascend());
                CenterViewport(gameState.Player.Position);
            }
            
            prevKeyboardState = keyboardState;
        }


        private void MovePlayerByClick(MouseState mouseState)
        {
            var bufferRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

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
            var level = gameState.CurrentFloor.Tiles;
            GraphicsDevice.Clear(Color.Black);

            // Begin draw
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var visible = gameState.Player.GetVisiblePoints(gameState.CurrentFloor);
            foreach (var point in visible)
            {
                spriteBatch.Draw(Textures[level[point.X, point.Y]], viewport.TranslatePoint(point.X, point.Y), Color.White);
            }
            DrawDoors(visible, Color.White);

            foreach (var point in gameState.Player.SeenPoints)
            {
                if (!visible.Contains(point))
                {
                    spriteBatch.Draw(Textures[level[point.X, point.Y]], viewport.TranslatePoint(point.X, point.Y), SeenTileColor);
                }
            }

            DrawDoors(gameState.Player.SeenPoints, SeenTileColor);

            // Paint path for player
            // TODO: Remove this after debugging
            if (gameState.Player.Path != null)
            {
                foreach (var point in gameState.Player.Path)
                {
                    spriteBatch.Draw(Textures[TextureKey.Path], viewport.TranslatePoint(point.X, point.Y), Color.White);
                }
            }

            // Draw characters
            foreach (var character in gameState.CurrentFloor.Characters)
            {
                if (visible.Contains(character.Position))
                {
                    spriteBatch.Draw(Textures[character.TextureKey], viewport.TranslatePoint(character.Position.X, character.Position.Y), Color.White);
                }
            }

            int topOffsetIndex = 0;
            foreach (var line in Log.GetLastLines(5))
            {
                spriteBatch.DrawString(font, line, new Vector2(3, 15 * topOffsetIndex++ + 3), Color.White);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawDoors(HashSet<Point> visiblePoints, Color color)
        {
            foreach (var door in gameState.CurrentFloor.Doors)
            {
                if (visiblePoints.Contains(door.Position))
                {
                    Texture2D texture = Textures[door.TextureKey];
                    spriteBatch.Draw(texture, viewport.TranslatePoint(door.Position.X, door.Position.Y), color);
                }
            }
        }
    }
}
