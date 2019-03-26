﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class Player : Movable
    {
        const bool CanRepeatWallJump = false;
        const bool CanDoubleJump = false;

        public bool FallThrough => Controller.MoveDown;

        private Controller Controller;
        private bool JumpKeyWasUp = false;
        private bool HasWallJumped = false;
        private bool HasDoubleJumped = false;
        private float LongJump = 0;

        private float Gravity = 800;
        private float MaxVel = 200;
        private float AccelRate = 600;
        private float JumpSpeed = 150;
        private float LongJumpSpeed = 250;
        private float LongJumpTime = 15;
        private float WallSlideFriction = 0.9F;

        public Player(Vector2 position, Game1 game):base(game, new Vector2(Chunk.TileSize, Chunk.TileSize))
        {
            Texture2D dummyTexture;
            dummyTexture = new Texture2D(game.GraphicsDevice, (int)Size.X, (int)Size.Y);
            Color[] colors = new Color[(int)(Size.X*Size.Y)];
            for(int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Color.Green;
            }
            dummyTexture.SetData(colors);
            Drawer = new AnimatedSprite(dummyTexture, 1, 1, game.SpriteBatch);

            this.Position = position;

            Controller = new Controller();
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            Controller.Update();
            bool Jump = Controller.Jump && JumpKeyWasUp;

            float dt = Game1.DeltaT;
            
            // Perform movement stepping. 
            // !! This code should never change Position !!
            if(Controller.MoveRight && Velocity.X < MaxVel)
            {
                Velocity.X += AccelRate * dt;
            }
            if(Controller.MoveLeft && -MaxVel < Velocity.X)
            {
                Velocity.X -= AccelRate * dt;
            }
            if (!(Controller.MoveLeft || Controller.MoveRight))
            {
                Velocity.X = 0;
            }
            
            Velocity.Y += dt * Gravity;
            
            // // Debug
            // if(Controller.MoveUp) Velocity.Y = -MaxVel;
            // else if(Controller.MoveDown) Velocity.Y = MaxVel;
            // else Velocity.Y = 0;

            if(Controller.Jump && LongJump > 0)
            {
                Velocity.Y -= AccelRate * dt;
            }

            if(LongJump > 0)
            {
                LongJump -= dt;
                if (Velocity.Y > 0)
                {
                    LongJump = 0;
                }
            }
            
            Object down = chunk.CollidePoint(new Vector2(Position.X+Size.X/2,
                                                         Position.Y+Size.Y+1));
            Object left = chunk.CollidePoint(new Vector2(Position.X       -1,
                                                         Position.Y+Size.Y/2));
            Object right= chunk.CollidePoint(new Vector2(Position.X+Size.X+1,
                                                         Position.Y+Size.Y/2));
            
            if (down != null)
            {
                HasDoubleJumped = false;
                HasWallJumped = false;
                if (Jump)
                {
                    Jump = false;
                    Velocity.Y -= JumpSpeed;
                    LongJump = LongJumpTime*dt;
                }
            }
            if (left != null || right != null)
            {
                HasWallJumped = false;
                if(Velocity.Y > 0)
                    Velocity.Y *= WallSlideFriction;

                if (Jump && (!HasWallJumped || CanRepeatWallJump) && right != null)
                {
                    Velocity.Y -= JumpSpeed;
                    Velocity.X = -MaxVel;
                    HasWallJumped = true;
                    Jump = false;
                }
                if (Jump && (!HasWallJumped || CanRepeatWallJump) && left != null)
                {
                    Velocity.Y -= JumpSpeed;
                    Velocity.X = MaxVel;
                    HasWallJumped = true;
                    Jump = false;
                }
            }

            JumpKeyWasUp = !Controller.Jump;
            
            // Now that all movement has been updated, check for collisions
            HandleCollisions(dt, chunk);
        }
    }
}
