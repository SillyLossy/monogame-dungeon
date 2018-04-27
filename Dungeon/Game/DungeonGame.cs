using Dungeon.Game.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Game.Entities;

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
        private const int LevelWidth = 100;
        private const int LevelHeight = 100;
        private readonly DungeonFloor floor;
        private readonly Entity player;
        private Point? selectedPoint;

        public DungeonGame()
        {
            floor = new DungeonFloor((int) DateTime.UtcNow.Ticks, LevelWidth, LevelHeight, 20, 15, 20, true, 2, 2);
            floor.GenerateLevel();
            player = floor.AddEntity();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = TileSize * LevelWidth;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = TileSize * LevelHeight;
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
                [DungeonTile.Floor] = CreateTexture(Color.WhiteSmoke),
                [DungeonTile.Stone] = CreateTexture(Color.Gray),
                [DungeonTile.Wall] = CreateTexture(Color.DodgerBlue),
                [TextureKey.Player] = CreateTexture(Color.Red),
                [TextureKey.Path] = CreateTexture(new Color(0, 255, 0, 50)),
                [TextureKey.Target] = CreateTexture(Color.Green)
            };

            // TODO: use this.Content to load your game content here
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
            
            MovePlayer();
            DoSteps();

            base.Update(gameTime);
        }

        private void DoSteps()
        {
            foreach (var entity in floor.Entities.Where(e => e.IsMoving))
            {
                entity.Step();
            }
        }

        private void MovePlayer()
        {
            var mouseState = Mouse.GetState(Window);
            var bufferRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // prevent processing mouse outside the window
            if (mouseState.LeftButton == ButtonState.Pressed && bufferRect.Contains(mouseState.X, mouseState.Y))
            {
                int x = mouseState.X / TileSize;
                int y = mouseState.Y / TileSize;
                Point? point = floor.Level[x, y] == DungeonTile.Floor ? new Point(x, y) : (Point?)null;
                if (point != null && point != selectedPoint)
                {
                    player.MoveTo(point.Value);
                    selectedPoint = point;
                }
            }

            if (!player.IsMoving)
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
            var level = floor.Level;
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            for (int x = 0; x < LevelWidth; x++)
            {
                for (int y = 0; y < LevelHeight; y++)
                {
                    spriteBatch.Draw(Textures[level[x,y]], new Vector2(x * TileSize, y * TileSize), Color.White);
                }
            }
            if (selectedPoint != null)
            {
                spriteBatch.Draw(Textures[TextureKey.Target], new Vector2(selectedPoint.Value.X * TileSize, selectedPoint.Value.Y * TileSize), Color.White);
                if (player.Path != null)
                {
                    foreach (var point in player.Path)
                    {
                        spriteBatch.Draw(Textures[TextureKey.Path], new Vector2(point.X * TileSize, point.Y * TileSize), Color.White);
                    }
                }
            }
            spriteBatch.Draw(Textures[TextureKey.Player], new Vector2(player.Position.X * TileSize, player.Position.Y * TileSize), Color.White);
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private Dictionary<object, Texture2D> Textures;
    }
}
