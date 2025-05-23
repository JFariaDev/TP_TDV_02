using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    public class Player
    {
        public Vector2 Position;
        private readonly float _speed;

        private Texture2D _walkTex, _idleTex;
        private int _direction;     // 0=down,1=left,2=right,3=up
        private int _frame, _frameCount = 4;
        private float _timer, _frameDuration = 0.15f;

        public Player(Vector2 spawn, float speed)
        {
            Position = spawn;
            _speed = speed;
        }

        public void LoadContent(ContentManager content)
        {
            _walkTex = content.Load<Texture2D>("Char_002");
            _idleTex = content.Load<Texture2D>("Char_002_Idle");
        }

        public void Update(GameTime gt)
        {
            var ks = Keyboard.GetState();
            Vector2 mov = Vector2.Zero;

            if (ks.IsKeyDown(Keys.W)) { mov.Y -= 1; _direction = 3; }
            else if (ks.IsKeyDown(Keys.S)) { mov.Y += 1; _direction = 0; }
            if (ks.IsKeyDown(Keys.A)) { mov.X -= 1; _direction = 1; }
            else if (ks.IsKeyDown(Keys.D)) { mov.X += 1; _direction = 2; }

            if (mov != Vector2.Zero)
            {
                mov.Normalize();
                Position += mov * _speed * (float)gt.ElapsedGameTime.TotalSeconds;

                // animação de andar
                _timer += (float)gt.ElapsedGameTime.TotalSeconds;
                if (_timer > _frameDuration)
                {
                    _frame = (_frame + 1) % _frameCount;
                    _timer = 0f;
                }
            }
            else
            {
                // reset frame para idle
                _frame = 0;
                _timer = 0f;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            // escolhe sprite sheet
            var tex = Keyboard.GetState().GetPressedKeys().Length > 0 ? _walkTex : _idleTex;

            // cada tile 24×24 no sheet de 16×16 pixels de base
            int tileSize = 24;
            var src = new Rectangle(
                _frame * tileSize,
                _direction * tileSize,
                tileSize, tileSize
            );

            sb.Draw(tex, Position, src, Color.White, 0f,
                    new Vector2(tileSize / 2, tileSize / 2),
                    1f, SpriteEffects.None, 0f);
        }
    }
}
