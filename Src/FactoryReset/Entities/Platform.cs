﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameManager
{
    class Platform : BoxEntity
    {
        public Platform(Vector2 position, Game1 game, int width, int height) : base(game, new Vector2(width, height))
        {
            Position = position;
        }
        
        public override void Draw()
        {
            Game.SpriteEngine.Draw(new Vector4(Position.X, Position.Y, Size.X, Size.Y));
        }
    }
}
