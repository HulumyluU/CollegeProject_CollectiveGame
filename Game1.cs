using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;


namespace CollectAThon_Final_Project_MAX_VRAJ
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Textures and Assets
        Texture2D backgroundTexture, obstacleTexture;
        List<Texture2D> playerIdleFrames, playerRunFrames, playerJumpFrames, playerHurtFrames, playerDeadFrames;
        List<Texture2D> collectibleFrames;
        Song backgroundMusic;

        // Player Variables
        Vector2 playerPosition;
        int playerFrameIndex = 0;
        float playerFrameTimer = 0f;
        string playerCurrentState = "Idle"; // States: Idle, Run, Jump, Hurt, Dead
        int playerHealth = 1;
        float playerScale = 0.3f; // Reduced player size
        bool facingRight = true; // Indicates the direction the player is facing

        // Collectibles and Obstacles
        List<Collectible> collectibles;
        List<Obstacle> obstacles;
        int collectibleFrameIndex = 0;
        float collectibleFrameTimer = 0f;

        // Game Variables
        int score = 0;
        enum GameState { Start, Playing, GameOver, HelpMenu, AboutScene }
        GameState currentGameState = GameState.Start;
        SpriteFont defaultFont;

        // Speed Progression Variables
        float baseSpeed = 0.5f;
        float speedMultiplier = 1.0f;
        float speedIncreaseInterval = 10f; // Increase speed every 10 seconds
        float speedTimer = 0f;
        //music


        // Randomizer
        Random random = new Random();

        // Collectible class to manage individual collectible movement
        class Collectible
        {
            public Vector2 Position { get; set; }
            public float Speed { get; set; }
            public Rectangle Bounds { get; set; }

            public Collectible(Vector2 position, float speed, Rectangle bounds)
            {
                Position = position;
                Speed = speed;
                Bounds = bounds;
            }

            public void Update(GameTime gameTime, int screenHeight, float speedMultiplier)
            {
                float adjustedSpeed = Speed * speedMultiplier;
                Position = new Vector2(Position.X, Position.Y + adjustedSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 50);

                // Reset position if collectible goes off-screen
                if (Position.Y > screenHeight)
                {
                    Position = new Vector2(Position.X, -700); // Start much higher above the screen
                }

                // Update bounds position
                Bounds = new Rectangle((int)Position.X, (int)Position.Y, Bounds.Width, Bounds.Height);
            }
        }

        // Obstacle class to manage individual obstacle movement
        class Obstacle
        {
            public Vector2 Position { get; set; }
            public float Speed { get; set; }
            public Rectangle Bounds { get; set; }

            public Obstacle(Vector2 position, float speed, Rectangle bounds)
            {
                Position = position;
                Speed = speed;
                Bounds = bounds;
            }

            public void Update(GameTime gameTime, int screenHeight, float speedMultiplier)
            {
                float adjustedSpeed = Speed * speedMultiplier;
                Position = new Vector2(Position.X, Position.Y + adjustedSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 50);

                // Reset position if obstacle goes off-screen
                if (Position.Y > screenHeight)
                {
                    Position = new Vector2(Position.X, -700); // Start much higher above the screen
                }

                // Update bounds position
                Bounds = new Rectangle((int)Position.X, (int)Position.Y, Bounds.Width, Bounds.Height);
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
        }

        protected override void Initialize()
        {
            playerPosition = new Vector2(400, 430);
            collectibles = new List<Collectible>();
            obstacles = new List<Obstacle>();
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            defaultFont = Content.Load<SpriteFont>("Fonts/DefaultFont");

            // Load Background
            backgroundTexture = Content.Load<Texture2D>("Background/Background");

            // Load Player Animations
            playerIdleFrames = LoadAnimationFrames("Player/Idle", 12);
            playerRunFrames = LoadAnimationFrames("Player/Run", 10);
            playerJumpFrames = LoadAnimationFrames("Player/Jump", 2);
            playerHurtFrames = LoadAnimationFrames("Player/Hurt", 6);
            playerDeadFrames = LoadAnimationFrames("Player/Dead", 10);

            // Load Collectible Frames
            collectibleFrames = LoadAnimationFrames("Collectible", 4);

            // Load Obstacle Texture
            obstacleTexture = Content.Load<Texture2D>("Obstacle/Boulder");


            backgroundMusic = Content.Load<Song>("Music/music1"); // Replace with your music file name

            // Start playing music
            MediaPlayer.IsRepeating = true; // Make music loop
            MediaPlayer.Volume = 0.5f; // Adjust volume (0.0 to 1.0)
            MediaPlayer.Play(backgroundMusic);

            // Generate Initial Collectibles
            GenerateCollectibles(5);

            // Generate Obstacles
            GenerateObstacles(3);
        }

        private List<Texture2D> LoadAnimationFrames(string folder, int frameCount)
        {
            var frames = new List<Texture2D>();
            for (int i = 0; i < frameCount; i++)
            {
                frames.Add(Content.Load<Texture2D>($"{folder}/frame{i}"));
            }
            return frames;
        }

        private void GenerateCollectibles(int count)
        {
            collectibles.Clear();
            for (int i = 0; i < count; i++)
            {
                float speed = baseSpeed + (float)random.NextDouble() * 1f;

                int collectibleWidth = collectibleFrames[0].Width / 4;
                int collectibleHeight = collectibleFrames[0].Height / 4;

                collectibles.Add(new Collectible(
                    new Vector2(random.Next(50, 750), -300), // Start above the screen
                    speed,
                    new Rectangle(0, 0, collectibleWidth, collectibleHeight)
                ));
            }
        }

        private void GenerateObstacles(int count)
        {
            obstacles.Clear();
            for (int i = 0; i < count; i++)
            {
                float speed = baseSpeed + (float)random.NextDouble() * 1f;

                int obstacleWidth = obstacleTexture.Width; // Use full width instead of dividing
                int obstacleHeight = obstacleTexture.Height; // Use full height

                obstacles.Add(new Obstacle(
                    new Vector2(random.Next(50, 750), -300),
                    speed,
                    new Rectangle(0, 0, obstacleWidth / 2, obstacleHeight / 2) // Slight reduction to maintain collision accuracy
                ));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
                Exit();
            switch (currentGameState)
            {
                case GameState.Start:
                    if (keyState.IsKeyDown(Keys.Enter))
                        currentGameState = GameState.Playing;
                    else if (keyState.IsKeyDown(Keys.Q))
                        currentGameState = GameState.HelpMenu;
                    else if (keyState.IsKeyDown(Keys.A))
                        currentGameState = GameState.AboutScene;
                    break;
                case GameState.Playing:
                    UpdatePlaying(gameTime);
                    break;

                case GameState.GameOver:
                    if (keyState.IsKeyDown(Keys.Enter))
                    {
                        ResetGame();
                        currentGameState = GameState.Start;
                    }
                    else if (keyState.IsKeyDown(Keys.Z))
                    {
                        SaveGameResults();
                    }
                    break;

                case GameState.HelpMenu:
                    if (keyState.IsKeyDown(Keys.M))
                        currentGameState = GameState.Start;
                    break;

                case GameState.AboutScene:
                    if (keyState.IsKeyDown(Keys.M))
                        currentGameState = GameState.Start;
                    break;




            }
            if (keyState.IsKeyDown(Keys.M)) // M key to mute/unmute
            {
                if (MediaPlayer.State == MediaState.Playing)
                    MediaPlayer.Pause();
                else
                    MediaPlayer.Resume();
            }

            base.Update(gameTime);
        }

        private void SaveGameResults()
        {
            try
            {
                // Define the file path to save the results
                string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameResults.txt");

                // Save game results to the file
                using (StreamWriter writer = new StreamWriter(savePath, true))
                {
                    writer.WriteLine("Game Results");
                    writer.WriteLine("-----------------");
                    writer.WriteLine($"Score: {score}");
                    writer.WriteLine($"Speed Multiplier: {speedMultiplier:F1}x");
                    writer.WriteLine($"Date: {DateTime.Now}");
                    writer.WriteLine();
                }

                Console.WriteLine($"Game results saved to {savePath}");
            }
            catch (Exception ex)
            {
                // Simple error handling
                Console.WriteLine($"Error saving game results: {ex.Message}");
            }
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            float animationSpeed = 0.1f;

            // Speed Progression Logic
            speedTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (speedTimer >= speedIncreaseInterval)
            {
                speedMultiplier += 0.2f; // Increase speed by 20% every 10 seconds
                speedTimer = 0f;
            }

            // Update Player Movement
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Left))
            {
                if (playerPosition.X > -40)
                {
                    playerPosition.X -= 5;
                    playerCurrentState = "Run";
                    facingRight = true;
                }
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                int playerWidth = (int)(playerIdleFrames[0].Width * playerScale - 22);
                if (playerPosition.X < GraphicsDevice.Viewport.Width - playerWidth + 37)
                {
                    playerPosition.X += 5;
                    playerCurrentState = "Run";
                    facingRight = false;
                }
            }
            else
            {
                playerCurrentState = "Idle";
            }

            // Handle Player Animation Timing
            playerFrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (playerFrameTimer >= animationSpeed)
            {
                playerFrameIndex++;
                playerFrameTimer = 0f;
            }

            if (playerCurrentState == "Idle" && playerFrameIndex >= playerIdleFrames.Count)
                playerFrameIndex = 0;
            else if (playerCurrentState == "Run" && playerFrameIndex >= playerRunFrames.Count)
                playerFrameIndex = 0;

            // Update Obstacles
            foreach (var obstacle in obstacles)
            {
                obstacle.Update(gameTime, GraphicsDevice.Viewport.Height, speedMultiplier);

                Rectangle playerBounds = new Rectangle(
                    (int)playerPosition.X + 10,
                    (int)playerPosition.Y + 10,
                    playerIdleFrames[0].Width / 3,
                    playerIdleFrames[0].Height / 1
                );

                if (obstacle.Position.Y > 0 && obstacle.Position.Y < GraphicsDevice.Viewport.Height)
                {
                    if (playerBounds.Intersects(obstacle.Bounds))
                    {
                        playerHealth--;
                        score -= 5;

                        if (playerHealth > 0)
                            playerCurrentState = "Hurt";
                        else
                        {
                            playerCurrentState = "Dead";
                            currentGameState = GameState.GameOver;
                        }
                    }
                }
            }

            // Update Collectibles
            for (int i = collectibles.Count - 1; i >= 0; i--)
            {
                collectibles[i].Update(gameTime, GraphicsDevice.Viewport.Height, speedMultiplier);

                Rectangle playerBounds = new Rectangle(
                    (int)playerPosition.X + 10,
                    (int)playerPosition.Y + 10,
                    playerIdleFrames[0].Width / 3,
                    playerIdleFrames[0].Height / 1
                );

                // Check for collision with collectibles
                if (playerBounds.Intersects(collectibles[i].Bounds))
                {
                    // Increase score by 10
                    score += 10;

                    // Remove the collected collectible
                    collectibles.RemoveAt(i);
                }
            }

            // Regenerate collectibles if list is empty
            if (collectibles.Count == 0)
            {
                GenerateCollectibles(5);
            }

            if (playerCurrentState == "Hurt" && playerFrameIndex >= playerHurtFrames.Count)
                playerCurrentState = "Idle";
        }

        private void ResetGame()
        {
            playerPosition = new Vector2(400, 430);
            score = 0;
            playerHealth = 1;
            speedMultiplier = 1.0f; // Reset speed multiplier
            speedTimer = 0f;

            GenerateCollectibles(5);
            GenerateObstacles(3);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            switch (currentGameState)
            {
                case GameState.Start:
                    DrawStartScreen();
                    break;

                case GameState.Playing:
                    DrawPlayingScreen(gameTime);
                    break;

                case GameState.GameOver:
                    DrawGameOverScreen();
                    break;

                case GameState.HelpMenu:
                    DrawHelpMenu();
                    break;

                case GameState.AboutScene:
                    DrawAboutScene();
                    break;
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawStartScreen()
        {
            spriteBatch.DrawString(defaultFont, "Collect-A-Thon Game", new Vector2(300, 200), Color.White);
            spriteBatch.DrawString(defaultFont, "Press Enter to Start", new Vector2(300, 240), Color.White);
            spriteBatch.DrawString(defaultFont, "Press Q for Help", new Vector2(300, 280), Color.White);
            spriteBatch.DrawString(defaultFont, "Press A for About", new Vector2(300, 320), Color.White);
            spriteBatch.DrawString(defaultFont, "Press ESK to close the Game any time", new Vector2(300, 360), Color.White);
        }

        private void DrawPlayingScreen(GameTime gameTime)
        {
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 480), Color.White);

            Texture2D currentFrame = playerIdleFrames[playerFrameIndex];
            if (playerCurrentState == "Run")
                currentFrame = playerRunFrames[playerFrameIndex];
            else if (playerCurrentState == "Hurt")
                currentFrame = playerHurtFrames[playerFrameIndex];
            else if (playerCurrentState == "Dead")
                currentFrame = playerDeadFrames[playerFrameIndex];

            SpriteEffects spriteEffect = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(currentFrame, playerPosition, null, Color.White, 0f, Vector2.Zero, playerScale, spriteEffect, 0f);

            // Draw Collectibles
            foreach (var collectible in collectibles)
            {
                // Only draw collectibles that are on screen
                if (collectible.Position.Y > 0 && collectible.Position.Y < GraphicsDevice.Viewport.Height)
                {
                    spriteBatch.Draw(
                        collectibleFrames[collectibleFrameIndex],
                        collectible.Position,
                        Color.White
                    );
                }
            }

            // Draw Obstacles
            foreach (var obstacle in obstacles)
            {
                if (obstacle.Position.Y > 0 && obstacle.Position.Y < GraphicsDevice.Viewport.Height)
                {
                    spriteBatch.Draw(obstacleTexture, obstacle.Position, null, Color.White, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0f);
                }
            }

            // Display Current Speed Multiplier
            spriteBatch.DrawString(defaultFont, $"Score: {score}", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(defaultFont, $"Health: {playerHealth}", new Vector2(10, 40), Color.White);
            spriteBatch.DrawString(defaultFont, $"Speed: {speedMultiplier:F1}x", new Vector2(10, 70), Color.White);
        }

        private void DrawGameOverScreen()
        {
            spriteBatch.DrawString(defaultFont, "Game Over!", new Vector2(350, 200), Color.White);
            spriteBatch.DrawString(defaultFont, $"Final Score: {score}", new Vector2(350, 240), Color.White);
            spriteBatch.DrawString(defaultFont, "Press Enter to Restart", new Vector2(300, 280), Color.White);
            spriteBatch.DrawString(defaultFont, "Press Z to Save Results", new Vector2(300, 320), Color.White);
            spriteBatch.DrawString(defaultFont, "Press ESK to close the Game any time", new Vector2(300, 360), Color.White);
        }

        private void DrawHelpMenu()
        {
            spriteBatch.DrawString(defaultFont, "Help Menu", new Vector2(350, 100), Color.White);
            spriteBatch.DrawString(defaultFont, "Game Instructions:", new Vector2(250, 150), Color.White);
            spriteBatch.DrawString(defaultFont, "- Use Left/Right Arrow Keys to Move", new Vector2(250, 200), Color.White);
            spriteBatch.DrawString(defaultFont, "- Collect Crystals to Increase Score", new Vector2(250, 230), Color.White);
            spriteBatch.DrawString(defaultFont, "- Avoid Obstacles", new Vector2(250, 260), Color.White);
            spriteBatch.DrawString(defaultFont, "- Speed Increases Over Time", new Vector2(250, 290), Color.White);
            spriteBatch.DrawString(defaultFont, "Press M to Return to Main Menu", new Vector2(250, 350), Color.White);
            spriteBatch.DrawString(defaultFont, "Press ESK to close the Game any time", new Vector2(300, 380), Color.White);
        }

        private void DrawAboutScene()
        {
            spriteBatch.DrawString(defaultFont, "Collect-A-Thon Game", new Vector2(300, 150), Color.White);
            spriteBatch.DrawString(defaultFont, "Developed by:", new Vector2(350, 200), Color.White);
            spriteBatch.DrawString(defaultFont, "Maksym Sovyk", new Vector2(350, 230), Color.White);
            spriteBatch.DrawString(defaultFont, "Vraj Joshi", new Vector2(350, 260), Color.White);
            spriteBatch.DrawString(defaultFont, "2024 All Rights Reserved", new Vector2(300, 300), Color.White);
            spriteBatch.DrawString(defaultFont, "Press M to Return to Main Menu", new Vector2(250, 350), Color.White);
        }
    }
}