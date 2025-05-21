using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian
{
    public class Map
    {
        private readonly Texture2D _tileset;
        private readonly int[,] _data;
        private readonly int _tileSize;
        private readonly int _tilesPerRow;

        public Map(Texture2D tileset, int[,] data, int tileSize)
        {
            _tileset = tileset;
            _data = data;
            _tileSize = tileSize;
            _tilesPerRow = tileset.Width / tileSize;
        }

        // Para que Game1.cs possa fazer map.Width / map.Height
        public int Width => _data.GetLength(1) * _tileSize;
        public int Height => _data.GetLength(0) * _tileSize;

        public void Draw(SpriteBatch sb)
        {
            int rows = _data.GetLength(0);
            int cols = _data.GetLength(1);

            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                {
                    int type = _data[y, x];
                    int idx;
                    float rot = 0f;

                    switch (type)
                    {
                        case 1: // TILE 1: chão
                            idx = 0; rot = 0f;
                            break;
                        case 2: // TILE 2: relva interior
                            idx = 1; rot = 0f;
                            break;
                        case 3: // TILE 3: obstáculo no chão (topo/baixo)
                            idx = 2; rot = 0f;
                            break;

                        // ---- Bordas entre cantos (TILE 6) ----
                        case 6:
                            idx = 5; // TILE 6 é o 5º índice (0-based)
                            if (y == 0)
                                rot = 0f;                  // topo
                            else if (x == 0)
                                rot = MathHelper.Pi;       // lateral esquerda (180°)
                            else if (y == rows - 1)
                                rot = MathHelper.PiOver2;  // fundo (90°)
                            else if (x == cols - 1)
                                rot = -MathHelper.PiOver2; // lateral direita (-90°)
                            break;

                        // ---- Cantos de relva (TILE 4 e 5) ----
                        case 4: // canto superior direito / inferior direito
                            idx = 3; // TILE 4 → índice 3
                            rot = (y == rows - 1)
                                  ? -MathHelper.PiOver2  // BR
                                  : 0f;                  // TR
                            break;
                        case 5: // canto superior esquerdo / inferior esquerdo
                            idx = 4; // TILE 5 → índice 4
                            rot = (y == rows - 1)
                                  ? MathHelper.PiOver2   // BL
                                  : 0f;                  // TL
                            break;

                        default:
                            idx = 0; rot = 0f;
                            break;
                    }

                    var srcRect = new Rectangle(
                        (idx % _tilesPerRow) * _tileSize,
                        (idx / _tilesPerRow) * _tileSize,
                        _tileSize, _tileSize);
                    var pos = new Vector2(x * _tileSize, y * _tileSize);
                    var origin = new Vector2(_tileSize / 2f, _tileSize / 2f);

                    sb.Draw(
                        _tileset,
                        pos + origin,
                        srcRect,
                        Color.White,
                        rot,
                        origin,
                        1f,
                        SpriteEffects.None,
                        0f
                    );
                }
        }
    }
}
