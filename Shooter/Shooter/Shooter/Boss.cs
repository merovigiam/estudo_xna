using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter
{
    class Boss
    {

        // Animation representing the Boss
        public Animation BossAnimation;

        // The position of the Boss ship relative to the top left corner of thescreen
        public Vector2 Position;

        // The state of the Boss Ship
        public bool Active;

        // The hit points of the Boss, if this goes to zero the Boss dies
        public int Health;

        // The amount of damage the Boss inflicts on the player ship
        public int Damage;

        // The amount of score the Boss will give to the player
        public int Value;

        // Get the width of the Boss ship
        public int Width
        {
            get { return BossAnimation.FrameWidth; }
        }

        // Get the height of the Boss ship
        public int Height
        {
            get { return BossAnimation.FrameHeight; }
        }

        // The speed at which the Boss moves
        float BossMoveSpeed;



        public void Initialize(Animation animation, Vector2 position)
        {
            // Load the Boss ship texture
            BossAnimation = animation;

            // Set the position of the Boss
            Position = position;

            // We initialize the Boss to be active so it will be update in the game
            Active = true;


            // Set the health of the Boss
            Health = 10;

            // Set the amount of damage the Boss can do
            Damage = 10;

            // Set how fast the Boss moves
            BossMoveSpeed = 6f;


            // Set the score value of the Boss
            Value = 100;

        }


        public void Update(GameTime gameTime)
        {
            // The Boss always moves to the left so decrement it's xposition
            Position.X -= BossMoveSpeed;

            // Update the position of the Animation
            BossAnimation.Position = Position;

            // Update Animation
            BossAnimation.Update(gameTime);

            // If the Boss is past the screen or its health reaches 0 then deactivateit
            if (Position.X < -Width || Health <= 0)
            {
                // By setting the Active flag to false, the game will remove this objet fromthe 
                // active game list
                Active = false;
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the animation
            BossAnimation.Draw(spriteBatch);
        }


    }
}
