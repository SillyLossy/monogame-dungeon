using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeon.Game
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class DungeonGame : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private const int TileSize = 8;
        private const int UpdatesPerSecond = 5;
        private const double TimeBetweenUpdates = 1000d / UpdatesPerSecond;
        private MouseState? prevMouseState;
        private Point? selectedPoint;
        private Action inputAction;
        private Dictionary<object, Texture2D> Textures;
        private TimeSpan lastUpdate;
        private DungeonGameState gameState;

        public DungeonGame()
        {
            gameState = new DungeonGameState();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = TileSize * gameState.CurrentFloor.Settings.Width;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = TileSize * gameState.CurrentFloor.Settings.Height;
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

            Textures = new Dictionary<object, Texture2D>
            {
                [DungeonTile.Floor] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.Floor)),
                [DungeonTile.Stone] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.Stone)),
                [DungeonTile.Wall] = Content.Load<Texture2D>(TextureKey.FromTile(DungeonTile.Wall)),
                [TextureKey.Player] = Content.Load<Texture2D>(TextureKey.Player),
                [TextureKey.Path] = CreateTexture(new Color(0, 255, 0, 50)),
                [TextureKey.Target] = CreateTexture(Color.Green),
                [DungeonTile.Test] = CreateTexture(Color.IndianRed)
            };
        }

        private Texture2D CreateTexture(Color color, int width = TileSize, int height = TileSize)
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
                return;
            }

            // Only do update if we have a pending action or the player is already moving.
            if (inputAction != null || gameState.Player.IsMoving)
            {
                inputAction?.Invoke();
                DoSteps();
            }

            lastUpdate = gameTime.TotalGameTime;
            inputAction = null;
            base.Update(gameTime);
        }
        
        private void DoSteps()
        {
            foreach (var entity in gameState.CurrentFloor.Entities.Where(e => e.IsMoving))
            {
                entity.Step();
            }
        }

        private void HandleMouse()
        {
            var mouseState = Mouse.GetState(Window);

            if (prevMouseState.HasValue)
            {
                if (prevMouseState.Value.LeftButton != mouseState.LeftButton)
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        inputAction = () => MovePlayerByClick(mouseState);
                    }
                }
            }

            prevMouseState = mouseState;
        }

        private void HandleKeyboard()
        {
            var keyboard = Keyboard.GetState();
            Direction? direction = null;

            if (keyboard.IsKeyDown(Keys.Left))
            {
                direction  = Direction.West;
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                direction = (Direction.East);
            }
            else if (keyboard.IsKeyDown(Keys.Up))
            {
                direction = (Direction.North);
            }
            else if (keyboard.IsKeyDown(Keys.Down))
            {
                direction = (Direction.South);
            }

            if (direction.HasValue)
            {
                inputAction = (() => MovePlayerByDirection(direction.Value));
            }
        }

        private void MovePlayerByDirection(Direction direction)
        {
            if (gameState.CurrentFloor.CanEntityMove(gameState.Player, direction))
            {
                gameState.Player.MoveTo(direction);
            }

        }

        private void MovePlayerByClick(MouseState mouseState)
        {
            var bufferRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // prevent processing mouse outside the window
            if (mouseState.LeftButton == ButtonState.Pressed && bufferRect.Contains(mouseState.X, mouseState.Y))
            {
                int x = mouseState.X / TileSize;
                int y = mouseState.Y / TileSize;
                Point? point = gameState.CurrentFloor.Tiles[x, y] == DungeonTile.Floor ? new Point(x, y) : (Point?)null;
                if (point != null && point != selectedPoint)
                {
                    gameState.Player.MoveTo(point.Value);
                    selectedPoint = point;
                }
            }

            if (!gameState.Player.IsMoving)
            {
                selectedPoint = null;
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

            spriteBatch.Begin();
            for (int x = 0; x < gameState.CurrentFloor.Settings.Width; x++)
            {
                for (int y = 0; y < gameState.CurrentFloor.Settings.Height; y++)
                {
                    spriteBatch.Draw(Textures[level[x, y]], new Vector2(x * TileSize, y * TileSize), Color.White);
                }
            }
            if (selectedPoint != null)
            {
                spriteBatch.Draw(Textures[TextureKey.Target], new Vector2(selectedPoint.Value.X * TileSize, selectedPoint.Value.Y * TileSize), Color.White);
                if (gameState.Player.Path != null)
                {
                    foreach (var point in gameState.Player.Path)
                    {
                        spriteBatch.Draw(Textures[TextureKey.Path], new Vector2(point.X * TileSize, point.Y * TileSize), Color.White);
                    }
                }
            }
            spriteBatch.Draw(Textures[TextureKey.Player], new Vector2(gameState.Player.Position.X * TileSize, gameState.Player.Position.Y * TileSize), Color.White);
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
