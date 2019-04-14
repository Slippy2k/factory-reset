﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System;

namespace team5
{
    class Alarm : GameObject
    {
        public enum AlarmState
        {
            Clear,
            Raised,
        };

        public bool Detected = false;
        //For Drone behavior
        public Vector2 LastKnowPos;
        public bool Drones = false;

        private AlarmState State = AlarmState.Clear;

        private float AlarmTime = 20;
        private float AlertTime = 1;
        private float Timer = 0;
        private AnimatedSprite Sprite;

        public Alarm(Game1 game) : base( game)
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(64,48));
        }

        public override void LoadContent(ContentManager content)
        {
            Game.SoundEngine.Load("Alert");
            Sprite.Texture = content.Load<Texture2D>("Textures/alert-backdrop");
            Sprite.Add("alert", 0, 1, 1.0);
        }


        public void SendDrones(Vector2 pos)
        {
            LastKnowPos = pos;
            Timer = AlarmTime;
            Drones = true;
        }

        public void SetState(AlarmState state )
        {
            State = state;
            switch (state)
            {
                case AlarmState.Clear:

                    break;
                case AlarmState.Raised:
                    Game.SoundEngine.Play("Alert");
                    break;
            }

        }

        public override void Update(Chunk chunk)
        {
            float dt = Game1.DeltaT;

            switch (State)
            {
                case AlarmState.Clear:
                    
                    if (Detected)
                    {
                        Timer = AlarmTime;
                        SetState(AlarmState.Raised);
                    }
                    break;
                case AlarmState.Raised:
                    Timer -= dt;
                    if (Timer <= 0)
                    {
                        Detected = false;
                        SetState(AlarmState.Clear);
                    }
                    break;
            }
        }

        public override void Draw()
        {
            //if(!Detected) return;
            Game.Transforms.PushView();
            Game.Transforms.ResetView();
            float textX = Game.GraphicsDevice.Viewport.Width / 2;
            float textY = Game.GraphicsDevice.Viewport.Height / 6;
            Sprite.Draw();
            Game.TextEngine.QueueText((Math.Floor(Timer)).ToString(), new Vector2(textX,textY), 
                                      "crashed-scoreboard", 32, TextEngine.Orientation.Center);
            Game.TextEngine.DrawText();
            Game.Transforms.PopView();
        }

    }
}
