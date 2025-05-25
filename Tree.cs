using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian2
{
    public class Tree
    {
        // Posição _tile_ (x*TileSize, y*TileSize), ou seja, a base da árvore
        public Vector2 Position;
        public Texture2D Texture;
        public bool IsInteractive;

        private const int TileSize = 16;

        public Tree(Texture2D tex, Vector2 pos, bool interactive)
        {
            Texture = tex;
            Position = pos;
            IsInteractive = interactive;
        }

        /// <summary>
        /// Retângulo inteiro onde a árvore é desenhada no mundo,
        /// com base no mesmo offset usado em Draw().
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                int drawX = (int)Position.X;
                // ajusta Dy para que a base (pé) da árvore fique na linha do tile
                int drawY = (int)Position.Y - (Texture.Height - TileSize);
                return new Rectangle(drawX, drawY, Texture.Width, Texture.Height);
            }
        }

        /// <summary>
        /// Desenha a árvore de modo que a base coincida com o tile.
        /// </summary>
        public void Draw(SpriteBatch sb)
        {
            var drawPos = new Vector2(
                Position.X,
                Position.Y - (Texture.Height - TileSize)
            );
            sb.Draw(Texture, drawPos, Color.White);
        }
    }
}
