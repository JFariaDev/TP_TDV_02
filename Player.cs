using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian2
{
    public class Player
    {
        public Vector2 Position;
        private Texture2D idleTex, walkTex;
        private float animTimer;
        private int animFrame;
        private int animDir; // 0-down, 1-left, 2-right, 3-up
        private bool isWalking;

        public Player(Texture2D idle, Texture2D walk)
        {
            idleTex = idle;
            walkTex = walk;
            Position = Vector2.Zero;
            animTimer = 0f;
            animFrame = 0;
            animDir = 0;
            isWalking = false;
        }

        public void Update(GameTime gameTime, KeyboardState ks)
        {
            Vector2 move = Vector2.Zero;

            // Prioridade: Cima > Baixo > Esquerda > Direita
            if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up))
            {
                move.Y -= 1;
                animDir = 3;
            }
            else if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down))
            {
                move.Y += 1;
                animDir = 0;
            }
            else if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))
            {
                move.X -= 1;
                animDir = 1;
            }
            else if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))
            {
                move.X += 1;
                animDir = 2;
            }

            if (move.LengthSquared() > 0)
            {
                Position += move * 2.5f; // Velocidade
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // Animação
            animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animTimer > 0.18f)
            {
                animTimer = 0f;
                animFrame = (animFrame + 1) % 4;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = isWalking ? walkTex : idleTex;
            int frame = animFrame;
            int dir = animDir;
            Rectangle src = new Rectangle(frame * 24, dir * 24, 24, 24);

            spriteBatch.Draw(
                tex,
                Position - new Vector2(12, 20),
                src,
                Color.White
            );
        }
    }
}
