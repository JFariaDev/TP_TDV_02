// Player.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    public enum Direction { Down = 0, Left = 1, Right = 2, Up = 3 }

    public class Player
    {
        private const int FrameCount = 4;
        private const float FrameDuration = 0.15f;

        private Texture2D _walkTex, _idleTex;
        private Vector2 _position;
        private float _speed;

        private int _cellWidth, _cellHeight;
        private Rectangle[,] _walkFrames, _idleFrames;

        private Direction _dir = Direction.Down;
        private int _walkFrame = 0, _idleFrame = 0;
        private float _walkTimer = 0f, _idleTimer = 0f;

        public Player(Vector2 startPosition, float speed = 120f)
        {
            _position = startPosition;
            _speed = speed;
        }

        public void LoadContent(ContentManager content)
        {
            _walkTex = content.Load<Texture2D>("Char_002");
            _idleTex = content.Load<Texture2D>("Char_002_Idle");

            // calcula o tamanho de cada célula (4 cols × 4 rows)
            _cellWidth = _walkTex.Width / FrameCount;  // ex: 96/4 = 24
            _cellHeight = _walkTex.Height / 4;           // ex: 96/4 = 24

            _walkFrames = new Rectangle[4, FrameCount];
            _idleFrames = new Rectangle[4, FrameCount];

            // cropa 2px do topo de cada célula para evitar bleed
            int marginTop = 2;
            int h = _cellHeight - marginTop;

            for (int d = 0; d < 4; d++)
                for (int f = 0; f < FrameCount; f++)
                {
                    var rect = new Rectangle(
                        f * _cellWidth,
                        d * _cellHeight + marginTop,
                        _cellWidth,
                        h
                    );
                    _walkFrames[d, f] = rect;
                    _idleFrames[d, f] = rect;
                }
        }

        public void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            Vector2 vel = Vector2.Zero;

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up)) { vel.Y = -1; _dir = Direction.Up; }
            else if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down)) { vel.Y = 1; _dir = Direction.Down; }
            else if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left)) { vel.X = -1; _dir = Direction.Left; }
            else if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) { vel.X = 1; _dir = Direction.Right; }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (vel != Vector2.Zero)
            {
                _position += vel * _speed * dt;
                _walkTimer += dt;
                if (_walkTimer >= FrameDuration)
                {
                    _walkTimer -= FrameDuration;
                    _walkFrame = (_walkFrame + 1) % FrameCount;
                }
                _idleFrame = 0; _idleTimer = 0f;
            }
            else
            {
                _idleTimer += dt;
                if (_idleTimer >= FrameDuration)
                {
                    _idleTimer -= FrameDuration;
                    _idleFrame = (_idleFrame + 1) % FrameCount;
                }
                _walkFrame = 0; _walkTimer = 0f;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            bool isIdle = (_walkFrame == 0 && _walkTimer == 0f);
            var tex = isIdle ? _idleTex : _walkTex;
            var frames = isIdle ? _idleFrames : _walkFrames;
            int frameIdx = isIdle ? _idleFrame : _walkFrame;

            sb.Draw(tex, _position, frames[(int)_dir, frameIdx], Color.White);
        }

        public Vector2 Position => _position;
    }
}

