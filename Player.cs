// Player.cs
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
        private int _direction;        // 0=down,1=left,2=right,3=up
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

        public void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();
            Vector2 mov = Vector2.Zero;

            // apenas movimento horizontal ou vertical, sem obliquo
            if (ks.IsKeyDown(Keys.A))
            {
                mov.X = -1;
                _direction = 1;  // left
            }
            else if (ks.IsKeyDown(Keys.D))
            {
                mov.X = 1;
                _direction = 2;  // right
            }
            else if (ks.IsKeyDown(Keys.W))
            {
                mov.Y = -1;
                _direction = 3;  // up
            }
            else if (ks.IsKeyDown(Keys.S))
            {
                mov.Y = 1;
                _direction = 0;  // down
            }

            if (mov != Vector2.Zero)
            {
                // normaliza para manter velocidade constante
                mov.Normalize();
                Position += mov * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                // anima walking
                _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timer > _frameDuration)
                {
                    _frame = (_frame + 1) % _frameCount;
                    _timer = 0f;
                }
            }
            else
            {
                // idle
                _frame = 0;
                _timer = 0f;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            // escolhe sheet
            bool walking = _timer > 0f;
            var tex = walking ? _walkTex : _idleTex;

            int tileSize = 24;
            var src = new Rectangle(
                _frame * tileSize,
                _direction * tileSize,
                tileSize, tileSize
            );

            sb.Draw(
                tex,
                Position,
                src,
                Color.White,
                0f,
                new Vector2(tileSize / 2, tileSize / 2),
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
