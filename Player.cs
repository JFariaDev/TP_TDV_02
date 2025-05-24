using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian2
{
    public class Player
    {
        public Vector2 Position;
        private Texture2D idleTex, walkTex;

        private int frame;
        private float animTimer;
        private int dir; // 0: Down, 1: Left, 2: Right, 3: Up
        private bool moving;

        public Player(Texture2D idle, Texture2D walk)
        {
            idleTex = idle;
            walkTex = walk;
            Position = Vector2.Zero;
            frame = 0;
            animTimer = 0f;
            dir = 0;
            moving = false;
        }

        public void Update(GameTime gameTime, KeyboardState state, MapZone zone)
        {
            float speed = 2f;
            Vector2 move = Vector2.Zero;
            int prevDir = dir;
            moving = false;

            if (state.IsKeyDown(Keys.Left))
            {
                move.X -= speed;
                dir = 1;
                moving = true;
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                move.X += speed;
                dir = 2;
                moving = true;
            }
            else if (state.IsKeyDown(Keys.Up))
            {
                move.Y -= speed;
                dir = 3;
                moving = true;
            }
            else if (state.IsKeyDown(Keys.Down))
            {
                move.Y += speed;
                dir = 0;
                moving = true;
            }

            // Eixo a eixo: evita atravessar cantos diagonais
            Vector2 newPos = Position;

            // Primeiro eixo X
            if (move.X != 0)
            {
                Vector2 tryPos = new Vector2(Position.X + move.X, Position.Y);
                int tx = (int)((tryPos.X + 12) / 16); // centraliza para hitbox, ajusta se necessário
                int ty = (int)((tryPos.Y + 12) / 16);
                if (!Game1.IsBlocked(zone, tx, ty))
                    newPos.X += move.X;
            }
            // Depois eixo Y
            if (move.Y != 0)
            {
                Vector2 tryPos = new Vector2(newPos.X, Position.Y + move.Y);
                int tx = (int)((tryPos.X + 12) / 16);
                int ty = (int)((tryPos.Y + 12) / 16);
                if (!Game1.IsBlocked(zone, tx, ty))
                    newPos.Y += move.Y;
            }

            moving = (newPos != Position);
            Position = newPos;

            // Animação idle e walk (idle também é animada!)
            animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animTimer >= 0.18f) // tempo por frame (ajusta se quiseres)
            {
                frame = (frame + 1) % 4;
                animTimer = 0f;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            int frameW = 24, frameH = 24;
            Rectangle srcRect = new Rectangle(frame * frameW, dir * frameH, frameW, frameH);
            Vector2 drawPos = Position + new Vector2(-4, -8); // centraliza sprite no tile

            if (moving)
                sb.Draw(walkTex, drawPos, srcRect, Color.White);
            else
                sb.Draw(idleTex, drawPos, srcRect, Color.White);
        }
    }
}
