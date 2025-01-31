﻿using System;
using Microsoft.Xna.Framework;

namespace GameManager
{
    class Camera
    {
        private Game1 Game;
        private Player Player;
        public Vector2 Position = new Vector2(0, 0);
        private Vector2 Velocity = new Vector2(0,0);
        private Chunk ChunkInFocus;
        private RectangleF ChunkClamps;
        public float Zoom = 1.0f;
        public float ViewScale{ get; private set; }
        // Target view half-size
        public const float TargetWidth = 30 * Chunk.TileSize / 2.0f;
        private Vector2 TargetSize = new Vector2(TargetWidth, 26*Chunk.TileSize/2.0f);

        private float ShakeDuration = 0;
        private float ShakeIntensity;

        public static float ScreenShakeMultiplier = 0.5F;

        private bool SnapOnNext = false;

        public Vector2 GetTargetSize()
        {
            return TargetSize;
        }

        public void Shake(float intensity, float duration = 1F)
        {
            ShakeDuration = Math.Max(ShakeDuration, duration);
            ShakeIntensity = Math.Max(ShakeIntensity, intensity);
        }

        public Camera(Player player, Game1 game)
        {
            Player = player;
            Game = game;
            ViewScale = 1;
        }

        private void UpdateClampData()
        {
            float lx = ChunkInFocus.BoundingBox.X;
            float ly = ChunkInFocus.BoundingBox.Y;
            float lw = ChunkInFocus.Size.X;
            float lh = ChunkInFocus.Size.Y;
            float cw = TargetSize.X;
            float ch = TargetSize.Y;
            ChunkClamps = new RectangleF(lx + cw, ly + ch, -2 * cw + lw * 2, -2 * ch + lh * 2);
        }

        public void Resize(int width, int height)
        {
            ViewScale = Zoom * width / (TargetSize.X*2);
            TargetSize.Y = (height / ViewScale) / 2;
            UpdateClampData();
        }   

        public void UpdateChunk(Chunk chunk)
        {
            ChunkInFocus = chunk;
            UpdateClampData();
        }

        public bool IsVisible(RectangleF target)
        {
            return new RectangleF(Position, TargetSize).Intersects(target);
        }

        public void SnapToLocation()
        {
            SnapOnNext = true;
        }

        public void UpdatePaused()
        {
            Vector2 camera = (TargetSize / Zoom) - Position;
            Game.Transforms.TranslateView(camera);
            Game.Transforms.ScaleView(ViewScale);
        }

        public void Update()
        {
            Vector2 intendedPosition = Player.Position;
            
            float clamp(float l, float x, float u) { return (x < l) ? l : (u < x) ? u : x; }
            intendedPosition.X = clamp(ChunkClamps.Left, intendedPosition.X, ChunkClamps.Right);
            intendedPosition.Y = clamp(ChunkClamps.Bottom, intendedPosition.Y, ChunkClamps.Top);

            if(ShakeDuration > 0)
            {
                bool xclamped = Position.X == clamp(ChunkClamps.Left, Position.X, ChunkClamps.Right);
                bool yclamped = Position.Y == clamp(ChunkClamps.Bottom, Position.Y, ChunkClamps.Top);
                if (xclamped || yclamped)
                {
                    Vector2 OrigPosition = Position;
                    do
                    {
                        Position = OrigPosition;
                        float dist = (float)Math.Sqrt(Game.RNG.NextDouble()) * ShakeIntensity * ScreenShakeMultiplier;
                        float angle = (float)Game.RNG.NextDouble() * 2 * (float)Math.PI;
                        Vector2 offset = dist * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                        Position += offset;
                    } while ((xclamped && (Position.X != clamp(ChunkClamps.Left, Position.X, ChunkClamps.Right)))
                    || (yclamped && (Position.Y != clamp(ChunkClamps.Bottom, Position.Y, ChunkClamps.Top))));
                }
                else
                {
                    float dist = (float)Math.Sqrt(Game.RNG.NextDouble()) * ShakeIntensity * ScreenShakeMultiplier;
                    float angle = (float)Game.RNG.NextDouble() * 2 * (float)Math.PI;
                    Vector2 offset = dist * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    Position += offset;
                }

                ShakeDuration -= Game1.DeltaT;
                if(ShakeDuration <= 0)
                {
                    ShakeIntensity = 0;
                }
            }

            // Ease towards intended position
            Vector2 direction = intendedPosition - Position;

            float length = (float)Math.Max(1.0, direction.Length());

            if (length <= 1 || SnapOnNext)
            {
                SnapOnNext = false;
                Position = intendedPosition;
            }
            else
            {
                float ease = (float)Math.Max(0.0, Math.Min(20.0, 0.2 + (Math.Pow(length, 1.5) / 100)));
                Position += direction * ease / length;
            }

            // Update view transform
            Vector2 camera = (TargetSize / Zoom)-Position;
            Game.Transforms.TranslateView(camera);
            Game.Transforms.ScaleView(ViewScale);
        }
    }
}
