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
        /// Ret�ngulo de colis�o / intera��o da tree.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X,
            (int)Position.Y - Texture.Height + TileSize,
            Texture.Width,
            Texture.Height);

        /// <summary>
        /// Cria uma nova �rvore.
        /// </summary>
        /// <param name="tex">Textura (verde ou azul, grande ou pequena).</param>
        /// <param name="pos">Posi��o de base (rodap�) da �rvore, em pixels.</param>
        /// <param name="interactive">Se � �azul� (True) ou n�o (False).</param>
        public Tree(Texture2D tex, Vector2 pos, bool interactive)
        {
            Texture = tex;
            Position = pos;
            IsInteractive = interactive;
        }

        /// <summary>
        /// Marca a �rvore como colhida e devolve esta inst�ncia
        /// para permitir express�es do tipo:
        /// promptTree.Pick().Texture = outraTextura;
        /// </summary>
        public Tree Pick()
        {
            IsInteractive = false;
            return this;
        }

        /// <summary>
        /// Desenha a �rvore.
        /// </summary>
        public void Draw(SpriteBatch sb)
            => sb.Draw(Texture, Position, Color.White);
    }
}
