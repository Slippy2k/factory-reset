﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;


namespace GameManager
{
    public abstract class Window
    {
        protected Game1 Game;

        public Window(Game1 game)
        {
            Game = game;
        }

        public virtual void LoadContent(ContentManager content)
        {}
        public virtual void UnloadContent()
        {}
        public abstract void Resize(int width, int height);
        public abstract void Update();
        public abstract void Draw();
    }
}
