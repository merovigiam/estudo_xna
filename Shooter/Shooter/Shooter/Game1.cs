using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;


//testando update do git
namespace Shooter
{
    /// <summary>
    /// Estados possiveis que o jogo pode entrar
    /// </summary>
    enum EstadoJogo { Parado, Menu, Jogando, Pontuacao };

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        EstadoJogo estado;

        Texture2D projectileTexture;
        List<Projectile> projectiles;

        Texture2D explosionTexture;
        List<Animation> explosions;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Player player;
        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        // A movement speed for the player
        float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;

        Texture2D mainMenuImage;
        Texture2D imagemGameOver;

        // Parallaxing Layers
        ParallaxBackground bgLayer1;
        ParallaxBackground bgLayer2;
        // Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        // The sound that is played when a laser is fired
        SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        SoundEffect explosionSound;

        // The music played during gameplay
        Song gameplayMusic;

        //Number that holds the player score
        int score;
        // The font used to display UI elements
        SpriteFont font;

        int posicaoCursor;

        int nivelTiro;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            nivelTiro = 1;
            posicaoCursor = 0;
            // TODO: Add your initialization logic here
            player = new Player();
            // Set a constant player move speed
            playerMoveSpeed = 8.0f;

            bgLayer1 = new ParallaxBackground();
            bgLayer2 = new ParallaxBackground();
            // Initialize the enemies list
            enemies = new List<Enemy>();

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            random = new Random();
            //Enable the FreeDrag gesture.
            //TouchPanel.EnabledGestures = GestureType.FreeDrag;

            projectiles = new List<Projectile>();
            explosions = new List<Animation>();
            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);
            //Set player's score to zero
            score = 0;
            estado = EstadoJogo.Menu;


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
            enemyTexture = Content.Load<Texture2D>("mineAnimation");
            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);

            // Load the parallaxing background
            bgLayer1.Initialize(Content, "bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "bgLayer2", GraphicsDevice.Viewport.Width, -2);

            projectileTexture = Content.Load<Texture2D>("laser");
            explosionTexture = Content.Load<Texture2D>("explosion");
            // Load the music
            gameplayMusic = Content.Load<Song>("sound/gameMusic");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("sound/explosion");

            // Start the music right away
            PlayMusic(gameplayMusic);
            // Load the score font
            font = Content.Load<SpriteFont>("gameFont");
            mainBackground = Content.Load<Texture2D>("mainbackground");

            
            mainMenuImage = Content.Load<Texture2D>("mainMenu");
            imagemGameOver = Content.Load<Texture2D>("endMenu");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            switch(estado) {
                case EstadoJogo.Menu:
                    updateMenuPrincipal();

                    break;

                case EstadoJogo.Jogando:

                    // Allows the game to exit
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                        this.Exit();

                    if (currentKeyboardState.IsKeyDown(Keys.Escape))
                        this.estado = EstadoJogo.Pontuacao;
                        

                    // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
                    

                    //Update the player
                    UpdatePlayer(gameTime);
                    bgLayer1.Update();
                    bgLayer2.Update();
                    // Update the enemies
                    UpdateEnemies(gameTime);

                    // Update the projectiles
                    UpdateProjectiles();
                    // Update the explosions
                    UpdateExplosions(gameTime);

                    // Update the collision
                    UpdateCollision();
                    

                    base.Update(gameTime);
                    break;

                case EstadoJogo.Parado:
                    this.Exit();
                    break;

                case EstadoJogo.Pontuacao:


                    updatePontuacao();
                    break;
            }
        }

        private void updateMenuPrincipal()
        {
            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                if (this.posicaoCursor == 0)
                    posicaoCursor++;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                if (this.posicaoCursor == 1)
                    posicaoCursor--;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Enter))
            {
                score = 0;
                if (posicaoCursor == 0)
                    estado = EstadoJogo.Jogando;
                else
                    estado = EstadoJogo.Parado;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                estado = EstadoJogo.Parado;

        }

        private void updatePontuacao()
        {
            if (currentKeyboardState.IsKeyDown(Keys.Space))
                this.estado = EstadoJogo.Menu;
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);

            // Windows Phone Controls
            //while (TouchPanel.IsGestureAvailable)
            //{
            //    GestureSample gesture = TouchPanel.ReadGesture();
            //    if (gesture.GestureType == GestureType.FreeDrag)
            //    {
            //        player.Position += gesture.Delta;
            //    }
            //}

            //// Get Thumbstick Controls
            //player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
            //player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

            // Use the Keyboard / Dpad
            if (currentKeyboardState.IsKeyDown(Keys.Left) ||  currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                player.Position.X -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                player.Position.X += playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                player.Position.Y -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                player.Position.Y += playerMoveSpeed;
            }

            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            if (nivelTiro == 1 && score > 1000)
                nivelTiro++;
            if (nivelTiro == 2 && score > 2000)
                nivelTiro++;
            

            // Fire only every interval we set as the fireTime
            if (gameTime.TotalGameTime - previousFireTime > fireTime)
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                // Play the laser sound
                laserSound.Play();
                AddProjectile(player.Position);
            }
            // reset score if player health goes to zero
            if (player.Health <= 0)
            {
                estado = EstadoJogo.Pontuacao;
                limparObjetosTela();
                nivelTiro = 1;
                player.Health = 100;
            }
        }

        private void limparObjetosTela()
        {
            projectiles.Clear();
            explosions.Clear();
            enemies.Clear();
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }
       
        private void UpdateEnemies(GameTime gameTime)
        {
            
            // Spawn a new enemy enemy every 1.5 seconds

           // if (score % 200 == 0 && (enemySpawnTime - new TimeSpan(0.1f) ) > 0.0)
             //       enemySpawnTime =  enemySpawnTime - new TimeSpan(0.1f);

            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);

                if (enemies[i].Active == false)
                {
                    // If not active and health <= 0
                    if (enemies[i].Health <= 0)
                    {
                        //Add to the player's score
                        score += enemies[i].Value;
                        // Play the explosion sound
                        explosionSound.Play();
                        // Add an explosion
                        AddExplosion(enemies[i].Position);
                    }

                    enemies.RemoveAt(i);

                }
            }
        }

        private void UpdateCollision()
        {
            // Use the Rectangle's built-in intersect function to 
            // determine if two objects are overlapping
            Rectangle rectangle1;
            Rectangle rectangle2;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.Width, player.Height);

            // Do the collision between the player and the enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                rectangle2 = new Rectangle((int)enemies[i].Position.X, (int)enemies[i].Position.Y, enemies[i].Width, enemies[i].Height);

                // Determine if the two objects collided with each
                // other
                if (rectangle1.Intersects(rectangle2))
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    player.Health -= enemies[i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies[i].Health = 0;

                    // If the player health is less than zero we died
                    if (player.Health <= 0)
                        player.Active = false;
                }

            }

            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                // Create the rectangles we need to determine if we collided with each other
                rectangle1 = new Rectangle((int)projectiles[i].Position.X // - projectiles[i].Width / 2
                    , (int)projectiles[i].Position.Y // - projectiles[i].Height / 2
                    , projectiles[i].Width, projectiles[i].Height);

                for (int j = 0; j < enemies.Count; j++)
                {
                    if (projectiles[i].Active)
                    {
                        rectangle2 = new Rectangle((int)enemies[j].Position.X,// - enemies[j].Width / 2,
                        (int)enemies[j].Position.Y// - enemies[j].Height / 2,
                        ,enemies[j].Width, enemies[j].Height);

                        // Determine if the two objects collided with each other
                        if (rectangle1.Intersects(rectangle2))
                        {
                            enemies[j].Health -= projectiles[i].Damage;
                            projectiles[i].Active = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            spriteBatch.Begin();

            switch (estado)
            {

                case EstadoJogo.Jogando:
                    spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

                    // Draw the moving background
                    bgLayer1.Draw(spriteBatch);
                    bgLayer2.Draw(spriteBatch);

                    // Draw the Player
                    player.Draw(spriteBatch);

                    // Draw the Enemies
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        enemies[i].Draw(spriteBatch);
                    }

                    // Draw the Projectiles
                    for (int i = 0; i < projectiles.Count; i++)
                    {
                        projectiles[i].Draw(spriteBatch);
                    }

                    // Draw the score
                    spriteBatch.DrawString(font, "score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
                    // Draw the player health
                    spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

                    // Draw the explosions
                    for (int i = 0; i < explosions.Count; i++)
                    {
                        explosions[i].Draw(spriteBatch);
                    }
                    break;

                case EstadoJogo.Menu:
                    spriteBatch.Draw(mainMenuImage, Vector2.Zero, Color.White);

                    spriteBatch.DrawString(font, "Comecar", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X+300, GraphicsDevice.Viewport.TitleSafeArea.Y+300), Color.White);
                    
                    spriteBatch.DrawString(font, "Sair", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + 300, GraphicsDevice.Viewport.TitleSafeArea.Y + 330), Color.White);
                    if(posicaoCursor == 0)
                        spriteBatch.DrawString(font, ">>>", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + 250, GraphicsDevice.Viewport.TitleSafeArea.Y + 300), Color.White);
                    else
                        spriteBatch.DrawString(font, ">>>", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + 250, GraphicsDevice.Viewport.TitleSafeArea.Y + 330), Color.White);

                    break;

                case EstadoJogo.Pontuacao:
                    spriteBatch.Draw(imagemGameOver, Vector2.Zero, Color.White);

                    spriteBatch.DrawString(font, "Pontuacao final : " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X+300, GraphicsDevice.Viewport.TitleSafeArea.Y+300), Color.White);
                    // Draw the player health
                    spriteBatch.DrawString(font, "FIM DE JOGO MANO!!!: ", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X+300, GraphicsDevice.Viewport.TitleSafeArea.Y + 330), Color.White);

                    break;

            }
            
            // Stop drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void AddProjectile(Vector2 position)
        {
            Projectile projectile;
            switch (nivelTiro)
            {
                case 0:
                case 1:
                    projectile = new Projectile();
                    projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position + new Vector2(100,35));
                    projectiles.Add(projectile);
                    break;

                case 2:
                    projectile = new Projectile();
                    projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position + new Vector2(100,0));
                    projectiles.Add(projectile);

                    projectile = new Projectile();
                    projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position + new Vector2(100,70));
                    projectiles.Add(projectile);

                    break;

                case 3:
                    projectile = new Projectile();
                    projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position + new Vector2(100,0));
                    projectiles.Add(projectile);

                    projectile = new Projectile();
                    projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position + new Vector2(100,35));
                    projectiles.Add(projectile);

                    projectile = new Projectile();
                    projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position + new Vector2(100,70));
                    projectiles.Add(projectile);


                    break;
            }
        }

        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }

        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }
    }
}
