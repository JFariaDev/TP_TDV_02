using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian
{
    /// <summary>
    /// Mapa infinito em chunks de 32x32 tiles, com ilhas de relva
    /// geradas proceduralmente (minimo 5x5, sem sub-ilhas <3x3).
    /// </summary>
    public class Map
    {
        public const int TileSize = 11;   // px por tile no ecra
        private const int ChunkSize = 32;  // cada chunk tem 32x32 tiles

        // parametros de geracao de ilhas
        private const int MinPatch = 5;
        private const int MinSub = 3;
        private const int SepTiles = 3;
        private const int CarveIter = 2;
        private const double CarveKeep = 0.6;

        private readonly int _seed;
        private readonly Dictionary<Point, Chunk> _chunks = new Dictionary<Point, Chunk>();
        private ContentManager _content;

        public Map(int seed = 0)
        {
            _seed = seed == 0 ? Environment.TickCount : seed;
        }

        /// <summary>
        /// Deve ser chamado em Game1.LoadContent
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _content = content;
            // pre-cria chunk (0,0)
            GetChunk(0, 0);
        }

        /// <summary>
        /// Desenha apenas o que cabe dentro de viewRect (px no mundo).
        /// </summary>
        public void Draw(SpriteBatch sb, Rectangle viewRect)
        {
            int tx0 = viewRect.Left / TileSize;
            int tx1 = viewRect.Right / TileSize + 1;
            int ty0 = viewRect.Top / TileSize;
            int ty1 = viewRect.Bottom / TileSize + 1;

            for (int ty = ty0; ty <= ty1; ty++)
                for (int tx = tx0; tx <= tx1; tx++)
                {
                    int cx = FloorDiv(tx, ChunkSize);
                    int cy = FloorDiv(ty, ChunkSize);
                    var chunk = GetChunk(cx, cy);

                    int lx = Mod(tx, ChunkSize);
                    int ly = Mod(ty, ChunkSize);

                    var dest = new Rectangle(
                        tx * TileSize, ty * TileSize,
                        TileSize, TileSize
                    );

                    // desenha dirt
                    sb.Draw(
                        chunk.DirtTex,
                        dest,
                        chunk.DirtSrc[chunk.DirtIndex[ly, lx]],
                        Color.White
                    );

                    // desenha relva com auto-tiling
                    if (chunk.Grass[ly, lx])
                    {
                        bool top = ly > 0 && chunk.Grass[ly - 1, lx];
                        bool bottom = ly < ChunkSize - 1 && chunk.Grass[ly + 1, lx];
                        bool left = lx > 0 && chunk.Grass[ly, lx - 1];
                        bool right = lx < ChunkSize - 1 && chunk.Grass[ly, lx + 1];

                        int row, col;
                        if (!top)
                        {
                            row = 0;
                            col = !left ? 0
                                : !right ? 3
                                : ((lx + ly) & 1) == 0 ? 1 : 2;
                        }
                        else if (!bottom)
                        {
                            row = 3;
                            col = !left ? 0
                                : !right ? 3
                                : ((lx + ly) & 1) == 0 ? 1 : 2;
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
                            int ch = chunk.Interior[ly, lx];
                            row = (ch < 2 ? 1 : 2);
                            col = (ch % 2 == 0 ? 1 : 2);
                        }

                        sb.Draw(
                            chunk.GrassTex,
                            dest,
                            chunk.GrassSrc[row, col],
                            Color.White
                        );
                    }
                }
        }

        /// <summary>
        /// Retorna true se o tile (tileX,tileY) for relva.
        /// </summary>
        public bool IsGrassTile(int tileX, int tileY)
        {
            int cx = tileX >= 0 ? tileX / ChunkSize : (tileX - ChunkSize + 1) / ChunkSize;
            int cy = tileY >= 0 ? tileY / ChunkSize : (tileY - ChunkSize + 1) / ChunkSize;
            var chunk = GetChunk(cx, cy);

            int lx = ((tileX % ChunkSize) + ChunkSize) % ChunkSize;
            int ly = ((tileY % ChunkSize) + ChunkSize) % ChunkSize;
            return chunk.Grass[ly, lx];
        }

        private Chunk GetChunk(int cx, int cy)
        {
            var key = new Point(cx, cy);
            if (!_chunks.TryGetValue(key, out var chunk))
            {
                chunk = new Chunk(cx, cy, ChunkSize, _seed);
                _chunks[key] = chunk;
                chunk.LoadContent(_content);
            }
            return chunk;
        }

        private static int FloorDiv(int a, int b)
            => (a >= 0 ? a : a - b + 1) / b;
        private static int Mod(int a, int b)
            => ((a % b) + b) % b;

        // cada chunk contem seu pedaco de mapa
        private class Chunk
        {
            public readonly bool[,] Grass;
            public readonly int[,] DirtIndex;
            public readonly int[,] Interior;
            public Texture2D DirtTex, GrassTex;
            public Rectangle[] DirtSrc;
            public Rectangle[,] GrassSrc;

            private static bool _loaded;
            private static Texture2D _dAtlas, _gAtlas;
            private static Rectangle[] _dSrc;
            private static Rectangle[,] _gSrc;

            public Chunk(int cx, int cy, int size, int seed)
            {
                var rnd = new Random(seed ^ cx ^ (cy << 16));
                Grass = new bool[size, size];
                DirtIndex = new int[size, size];
                Interior = new int[size, size];

                // dirt aleatorio
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        DirtIndex[y, x] = rnd.Next(6);

                // gera 2-4 retangulos
                var rects = new List<Rectangle>();
                int targ = rnd.Next(2, 5), tries = targ * 5;
                while (rects.Count < targ && tries-- > 0)
                {
                    int w = rnd.Next(MinPatch, size / 2);
                    int h = rnd.Next(MinPatch, size / 2);
                    int x0 = rnd.Next(0, size - w);
                    int y0 = rnd.Next(0, size - h);
                    var r = new Rectangle(x0, y0, w, h);

                    bool ok = true;
                    foreach (var o in rects)
                    {
                        var inf = new Rectangle(
                            o.X - SepTiles, o.Y - SepTiles,
                            o.Width + SepTiles * 2,
                            o.Height + SepTiles * 2
                        );
                        if (inf.Intersects(r)) { ok = false; break; }
                    }
                    if (ok) rects.Add(r);
                }

                // carve + remove sub-ilhas pequenas
                foreach (var r in rects)
                {
                    bool[,] mask = new bool[r.Height, r.Width];
                    for (int yy = 0; yy < r.Height; yy++)
                        for (int xx = 0; xx < r.Width; xx++)
                            mask[yy, xx] = true;

                    for (int it = 0; it < CarveIter; it++)
                    {
                        var nxt = new bool[r.Height, r.Width];
                        for (int yy = 0; yy < r.Height; yy++)
                            for (int xx = 0; xx < r.Width; xx++)
                            {
                                if (!mask[yy, xx]) continue;
                                bool all8 = true;
                                for (int dy = -1; dy <= 1; dy++)
                                    for (int dx = -1; dx <= 1; dx++)
                                    {
                                        if (dx == 0 && dy == 0) continue;
                                        int nx = xx + dx, ny = yy + dy;
                                        if (nx < 0 || nx >= r.Width || ny < 0 || ny >= r.Height
                                            || !mask[ny, nx])
                                        { all8 = false; break; }
                                    }
                                nxt[yy, xx] = all8
                                    ? true
                                    : rnd.NextDouble() < CarveKeep;
                            }
                        mask = nxt;
                    }

                    RemoveSmall(mask, MinSub, MinSub);

                    for (int yy = 0; yy < r.Height; yy++)
                        for (int xx = 0; xx < r.Width; xx++)
                            if (mask[yy, xx])
                                Grass[r.Y + yy, r.X + xx] = true;
                }

                // auto-tiling interior
                int w0 = 4, w1 = 3, w2 = 1, w3 = 1, tot = w0 + w1 + w2 + w3;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        if (Grass[y, x]
                         && y > 0 && y < size - 1 && x > 0 && x < size - 1
                         && Grass[y - 1, x] && Grass[y + 1, x]
                         && Grass[y, x - 1] && Grass[y, x + 1])
                        {
                            int r = rnd.Next(tot);
                            Interior[y, x] =
                                r < w0 ? 0 :
                                r < w0 + w1 ? 1 :
                                r < w0 + w1 + w2 ? 2 : 3;
                        }
                        else Interior[y, x] = -1;
                    }
            }

            public void LoadContent(ContentManager cm)
            {
                if (!_loaded)
                {
                    _dAtlas = cm.Load<Texture2D>("dirt");
                    _dSrc = new Rectangle[6];
                    for (int i = 0; i < 6; i++)
                        _dSrc[i] = new Rectangle(i * 16, 0, 16, 16);

                    _gAtlas = cm.Load<Texture2D>("grass2");
                    _gSrc = new Rectangle[4, 4];
                    for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++)
                            _gSrc[r, c] = new Rectangle(c * 16, r * 16, 16, 16);

                    _loaded = true;
                }
                DirtTex = _dAtlas; DirtSrc = _dSrc;
                GrassTex = _gAtlas; GrassSrc = _gSrc;
            }

            private static void RemoveSmall(bool[,] m, int mw, int mh)
            {
                int h = m.GetLength(0), w = m.GetLength(1);
                var vis = new bool[h, w];
                var dirs = new[]{new Point(1,0),new Point(-1,0),
                               new Point(0,1),new Point(0,-1)};
                for (int yy = 0; yy < h; yy++) for (int xx = 0; xx < w; xx++)
                    {
                        if (!m[yy, xx] || vis[yy, xx]) continue;
                        var q = new Queue<Point>();
                        var zone = new List<Point>();
                        vis[yy, xx] = true; q.Enqueue(new Point(xx, yy));
                        while (q.Count > 0)
                        {
                            var p = q.Dequeue(); zone.Add(p);
                            foreach (var d in dirs)
                            {
                                int nx = p.X + d.X, ny = p.Y + d.Y;
                                if (nx >= 0 && nx < w && ny >= 0 && ny < h
                                    && m[ny, nx] && !vis[ny, nx])
                                {
                                    vis[ny, nx] = true; q.Enqueue(new Point(nx, ny));
                                }
                            }
                        }
                        int minX = w, maxX = 0, minY = h, maxY = 0;
                        foreach (var p in zone)
                        {
                            minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                            minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
                        }
                        if (maxX - minX + 1 < mw || maxY - minY + 1 < mh)
                            foreach (var p in zone) m[p.Y, p.X] = false;
                    }
            }
        }
    }
}
