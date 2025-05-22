// Map.cs
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian
{
    public class Map
    {
        public const int TileSize = 11;  // cada célula no ecrã mede 11×11 px
        private const int GrassTileSize = 16;  // dimensão real de cada tile em grass2.png
        private const int PathWidth = 5;   // largura constante dos caminhos (ímpar p/ centrar)

        private readonly bool[,] _grass;       // true = grama
        private readonly bool[,] _path;        // true = chão

        private Texture2D _dirtTex, _grassTex;
        private readonly Rectangle[] _dirtSrc = new Rectangle[6];
        private readonly Rectangle[,] _grassSrc = new Rectangle[4, 4];
        private readonly int[,] _dirtIndex;
        private readonly int[,] _interiorChoice;
        // -1 = não interior completo
        //  0 = topo-esq   (row=1,col=1)
        //  1 = topo-dir   (row=1,col=2)
        //  2 = base-esq   (row=2,col=1)
        //  3 = base-dir   (row=2,col=2)

        public readonly int Width, Height;

        public Map(bool[,] grass)
        {
            _grass = grass;
            Height = grass.GetLength(0);
            Width = grass.GetLength(1);
            _path = new bool[Height, Width];
            _dirtIndex = new int[Height, Width];
            _interiorChoice = new int[Height, Width];

            var rnd = new Random(12345);

            // 1) índice aleatório de dirt (0–5)
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _dirtIndex[y, x] = rnd.Next(6);

            // 2) gera alguns caminhos lineares com espaçamentos aleatórios,
            //    garantindo zonas de grama >=5×5
            GenerateLinearGridPaths(rnd);

            // 3) escava o caminho (remove grama)
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (_path[y, x])
                        _grass[y, x] = false;

            // 4) remove manchas de grama <5×5
            RemoveSmallGrassZones(minWidth: 5, minHeight: 5);

            // 5) pré-calcula interiores de grama para auto-tiling
            int w0 = 4, w1 = 3, w2 = 1, w3 = 1, total = w0 + w1 + w2 + w3;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    bool interior = _grass[y, x]
                                 && y > 0 && y < Height - 1
                                 && x > 0 && x < Width - 1
                                 && _grass[y - 1, x] && _grass[y + 1, x]
                                 && _grass[y, x - 1] && _grass[y, x + 1];

                    if (!interior) _interiorChoice[y, x] = -1;
                    else
                    {
                        int r = rnd.Next(total);
                        _interiorChoice[y, x] =
                            r < w0 ? 0 :
                            r < w0 + w1 ? 1 :
                            r < w0 + w1 + w2 ? 2 : 3;
                    }
                }
        }

        private void GenerateLinearGridPaths(Random rnd)
        {
            // vamos escolher entre 2 e 4 caminhos horizontais e 2 a 4 verticais,
            // mas com espaçamento aleatório garantindo faixas de grama ≥5
            int half = PathWidth / 2;

            // horizontais
            int hCount = rnd.Next(2, 5);
            var chosenY = new List<int>();
            while (chosenY.Count < hCount)
            {
                int y = rnd.Next(half, Height - half);
                bool ok = true;
                foreach (var yy in chosenY)
                    if (Math.Abs(yy - y) < PathWidth + 5) { ok = false; break; }
                if (ok) chosenY.Add(y);
            }
            // desenha cada horizontal
            foreach (var yCenter in chosenY)
            {
                for (int dy = -half; dy <= half; dy++)
                {
                    int yy = yCenter + dy;
                    if (yy < 0 || yy >= Height) continue;
                    for (int x = 0; x < Width; x++)
                        _path[yy, x] = true;
                }
            }

            // verticais
            int vCount = rnd.Next(2, 5);
            var chosenX = new List<int>();
            while (chosenX.Count < vCount)
            {
                int x = rnd.Next(half, Width - half);
                bool ok = true;
                foreach (var xx in chosenX)
                    if (Math.Abs(xx - x) < PathWidth + 5) { ok = false; break; }
                if (ok) chosenX.Add(x);
            }
            // desenha cada vertical
            foreach (var xCenter in chosenX)
            {
                for (int dx = -half; dx <= half; dx++)
                {
                    int xx = xCenter + dx;
                    if (xx < 0 || xx >= Width) continue;
                    for (int y = 0; y < Height; y++)
                        _path[y, xx] = true;
                }
            }
        }

        private void RemoveSmallGrassZones(int minWidth, int minHeight)
        {
            var visited = new bool[Height, Width];
            for (int y0 = 0; y0 < Height; y0++)
                for (int x0 = 0; x0 < Width; x0++)
                {
                    if (!_grass[y0, x0] || visited[y0, x0]) continue;
                    var queue = new Queue<Point>();
                    var zone = new List<Point>();
                    visited[y0, x0] = true;
                    queue.Enqueue(new Point(x0, y0));
                    while (queue.Count > 0)
                    {
                        var p = queue.Dequeue();
                        zone.Add(p);
                        foreach (var d in new[] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0) })
                        {
                            int nx = p.X + d.X, ny = p.Y + d.Y;
                            if (nx >= 0 && nx < Width && ny >= 0 && ny < Height
                               && _grass[ny, nx] && !visited[ny, nx])
                            {
                                visited[ny, nx] = true;
                                queue.Enqueue(new Point(nx, ny));
                            }
                        }
                    }
                    // bounding box
                    int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
                    foreach (var p in zone)
                    {
                        minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                        minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
                    }
                    if (maxX - minX + 1 < minWidth || maxY - minY + 1 < minHeight)
                    {
                        foreach (var p in zone)
                            _grass[p.Y, p.X] = false;
                    }
                }
        }

        public void LoadContent(ContentManager content)
        {
            // dirt atlas 6×1 de 16×16
            _dirtTex = content.Load<Texture2D>("dirt");
            for (int i = 0; i < 6; i++)
                _dirtSrc[i] = new Rectangle(i * 16, 0, 16, 16);

            // grass2 atlas 4×4 de 16×16
            _grassTex = content.Load<Texture2D>("grass2");
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    _grassSrc[r, c] = new Rectangle(c * GrassTileSize, r * GrassTileSize, GrassTileSize, GrassTileSize);
        }

        public void Draw(SpriteBatch sb)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var dest = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);

                    // 1) chão de base
                    sb.Draw(_dirtTex, dest, _dirtSrc[0], Color.White);

                    // 2) caminho?
                    if (_path[y, x])
                    {
                        sb.Draw(_dirtTex, dest, _dirtSrc[1], Color.White);
                        continue;
                    }

                    // 3) relva com auto-tiling
                    if (_grass[y, x])
                    {
                        bool top = (y > 0 && _grass[y - 1, x]);
                        bool bottom = (y < Height - 1 && _grass[y + 1, x]);
                        bool left = (x > 0 && _grass[y, x - 1]);
                        bool right = (x < Width - 1 && _grass[y, x + 1]);

                        int row, col;
                        if (!top)
                        {
                            row = 0; col = !left ? 0 : !right ? 3 : (((x + y) & 1) == 0 ? 1 : 2);
                        }
                        else if (!bottom)
                        {
                            row = 3; col = !left ? 0 : !right ? 3 : (((x + y) & 1) == 0 ? 1 : 2);
                        }
                        else if (!left)
                        {
                            row = 1; col = 0;
                        }
                        else if (!right)
                        {
                            row = 1; col = 3;
                        }
                        else
                        {
                            int ch = _interiorChoice[y, x];
                            row = (ch < 2 ? 1 : 2);
                            col = (ch % 2 == 0 ? 1 : 2);
                        }
                        sb.Draw(_grassTex, dest, _grassSrc[row, col], Color.White);
                    }
                }
        }
    }
}
