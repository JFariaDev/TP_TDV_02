using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    public class Player
    {
        public Vector2 Position;
        private readonly Texture2D _texture;
        private readonly float _speed = 100f;
        private readonly float _scale;
        private readonly int _tileSize;
        private readonly HashSet<int> _impassables;

        public Player(Texture2D texture, Vector2 startPosition, int tileSize, HashSet<int> impassables)
        {
            _texture = texture;
            Position = startPosition;
            _tileSize = tileSize;
            _impassables = impassables;
            _scale = tileSize / (float)texture.Width;
        }

        public void Update(GameTime gameTime, int[,] map)
        {
            var kb = Keyboard.GetState();
            Vector2 move = Vector2.Zero;
            if (kb.IsKeyDown(Keys.W)) move.Y -= 1;
            if (kb.IsKeyDown(Keys.S)) move.Y += 1;
            if (kb.IsKeyDown(Keys.A)) move.X -= 1;
            if (kb.IsKeyDown(Keys.D)) move.X += 1;
            if (move != Vector2.Zero) move.Normalize();

            Vector2 delta = move * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            TryMove(new Vector2(delta.X, 0), map);
            TryMove(new Vector2(0, delta.Y), map);
        }

        private void TryMove(Vector2 delta, int[,] map)
        {
            Vector2 newPos = Position + delta;
            var bounds = new Rectangle((int)newPos.X, (int)newPos.Y, _tileSize, _tileSize);

            int left = bounds.Left / _tileSize;
            int right = (bounds.Right - 1) / _tileSize;
            int top = bounds.Top / _tileSize;
            int bottom = (bounds.Bottom - 1) / _tileSize;

            for (int y = top; y <= bottom; y++)
                for (int x = left; x <= right; x++)
                {
                    if (y < 0 || y >= map.GetLength(0) ||
                        x < 0 || x >= map.GetLength(1) ||
                        _impassables.Contains(map[y, x]))
                    {
                        return; // colisão
                    }
                }

            Position = newPos;
        }

        public void Draw(SpriteBatch sb)
        {
            var origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            sb.Draw(
                _texture,
                Position + origin * _scale,
                null,
                Color.White,
                0f,
                origin,
                _scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}
