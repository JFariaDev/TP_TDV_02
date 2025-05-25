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
        public int Width;
        public int Height;

        public Player(Texture2D idle, Texture2D walk)
        {
            idleTex = idle;
            walkTex = walk;
            Position = Vector2.Zero;
            frame = 0;
            animTimer = 0f;
            dir = 0;
            moving = false;

            // tamanho do sprite de colisão e draw
            Width = 24;
            Height = 24;
        }

        public void Update(GameTime gameTime, KeyboardState state, MapZone zone)
        {
            float speed = 2f;
            Vector2 move = Vector2.Zero;

            // Input e direção
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
            else
            {
                moving = false;
            }

            // aplica o movimento (colisões com ginásios e árvores ficam em Game1)
            Position += move;

            // animação
            animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animTimer >= 0.18f)
            {
                frame = (frame + 1) % 4;
                animTimer = 0f;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            const int frameW = 24;
            const int frameH = 24;

            // retângulo do frame
            Rectangle src = new Rectangle(
                frame * frameW,
                dir * frameH,
                frameW,
                frameH);

            // centraliza ajuste
            Vector2 drawPos = Position + new Vector2(-4, -8);

            // desenha
            sb.Draw(
                moving ? walkTex : idleTex,
                drawPos,
                src,
                Color.White);
        }
    }
}
