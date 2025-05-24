using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian2
{
    public class Tree
    {
        public Vector2 Position;
        public Texture2D Texture;
        public bool IsInteractive;

        public int Width => Texture.Width;
        public int Height => Texture.Height;

        public Rectangle Bounds =>
            new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public Tree(Texture2D tex, Vector2 pos, bool interactive)
        {
            Texture = tex;
            Position = pos;
            IsInteractive = interactive;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Texture, Position, Color.White);
        }
    }
}
