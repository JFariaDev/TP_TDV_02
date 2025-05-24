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
        private int dir;       // 0 = down, 1 = left, 2 = right, 3 = up
        private bool moving;
        public int width;
        public int height;



        public Player(Texture2D idle, Texture2D walk)
        {
            idleTex = idle;
            walkTex = walk;
            Position = Vector2.Zero;
            frame = 0;
            animTimer = 0f;
            dir = 0;
            moving = false;
            width = 24;
            height = 24;
        }

        public void Update(GameTime gameTime, KeyboardState state, MapZone zone)
        {
            float speed = 2f;
            Vector2 move = Vector2.Zero;

            // Input e direção
            if (state.IsKeyDown(Keys.Left)) { move.X -= speed; dir = 1; moving = true; }
            else if (state.IsKeyDown(Keys.Right)) { move.X += speed; dir = 2; moving = true; }
            else if (state.IsKeyDown(Keys.Up)) { move.Y -= speed; dir = 3; moving = true; }
            else if (state.IsKeyDown(Keys.Down)) { move.Y += speed; dir = 0; moving = true; }
            else moving = false;

            Vector2 newPos = Position;

            // Primeiro eixo X
            if (move.X != 0)
            {
                var tryPos = new Vector2(Position.X + move.X, Position.Y);
                int tx = (int)((tryPos.X + 12) / 16);
                int ty = (int)((tryPos.Y + 12) / 16);
                if (!Game1.IsBlocked(zone, tx, ty))
                    newPos.X += move.X;
            }
            // Depois eixo Y
            if (move.Y != 0)
            {
                var tryPos = new Vector2(newPos.X, Position.Y + move.Y);
                int tx = (int)((tryPos.X + 12) / 16);
                int ty = (int)((tryPos.Y + 12) / 16);
                if (!Game1.IsBlocked(zone, tx, ty))
                    newPos.Y += move.Y;
            }

            Position = newPos;

            // Animação
            animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animTimer >= 0.18f)
            {
                frame = (frame + 1) % 4;
                animTimer = 0f;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            const int frameW = 24, frameH = 24;
            // Retângulo da frame correta
            Rectangle src = new Rectangle(frame * frameW, dir * frameH, frameW, frameH);
            // Ajuste de posição para centrar
            Vector2 drawPos = Position + new Vector2(-4, -8);

            // Desenha idle ou walk conforme o estado
            sb.Draw(moving ? walkTex : idleTex, drawPos, src, Color.White);
        }

        

    }

}
