using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian2
{
    public class Tree
    {
        public Vector2 Position;
        public Texture2D Texture;
        public bool IsInteractive;

        private const int TileSize = 16;

        /// <summary>
        /// Retângulo de colisão / interação da tree.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X,
            (int)Position.Y - Texture.Height + TileSize,
            Texture.Width,
            Texture.Height);

        /// <summary>
        /// Cria uma nova árvore.
        /// </summary>
        /// <param name="tex">Textura (verde ou azul, grande ou pequena).</param>
        /// <param name="pos">Posição de base (rodapé) da árvore, em pixels.</param>
        /// <param name="interactive">Se é “azul” (True) ou não (False).</param>
        public Tree(Texture2D tex, Vector2 pos, bool interactive)
        {
            Texture = tex;
            Position = pos;
            IsInteractive = interactive;
        }

        /// <summary>
        /// Marca a árvore como colhida e devolve esta instância
        /// para permitir expressões do tipo:
        /// promptTree.Pick().Texture = outraTextura;
        /// </summary>
        public Tree Pick()
        {
            IsInteractive = false;
            return this;
        }

        /// <summary>
        /// Desenha a árvore.
        /// </summary>
        public void Draw(SpriteBatch sb)
            => sb.Draw(Texture, Position, Color.White);
    }
}
