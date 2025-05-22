// Map.cs
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian
{
    /// <summary>
    /// Mapa infinito por chunks de 64×64 tiles, com relva gerada via Perlin Noise
    /// (suave e contínuo) e auto-tiling de bordas/interior usando grass2.png.
    /// </summary>
    public class Map
    {
        public const int TileSize = 11;
        private const int ChunkSize = 64;

        // Parâmetros do Perlin
        private const int Octaves = 4;
        private const double Lacunarity = 2.0;
        private const double Persistence = 0.5;
        private const double NoiseScale = 0.05;  // aumenta a frequência
        private const double Threshold = 0.0;   // relva onde noise > 0

        private readonly int _seed;
        private readonly Dictionary<Point, Chunk> _chunks = new();
        private ContentManager _content;

        public Map(int seed)
        {
            _seed = seed;
        }

        /// <summary>
        /// Deve ser chamado uma vez no LoadContent do Game1.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _content = content;
            // força a criar e carregar o primeiro chunk
            GetChunk(0, 0);
        }

        /// <summary>
        /// Desenha todos os tiles dentro de viewRect (coordenadas de mundo, em px).
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
                    // calcula que chunk e que posição local
                    int cx = FloorDiv(tx, ChunkSize);
                    int cy = FloorDiv(ty, ChunkSize);
                    var chunk = GetChunk(cx, cy);

                    int lx = Mod(tx, ChunkSize);
                    int ly = Mod(ty, ChunkSize);

                    var dest = new Rectangle(
                        tx * TileSize,
                        ty * TileSize,
                        TileSize,
                        TileSize
                    );

                    // 1) dirt de base
                    int d = chunk.DirtIndex[ly, lx];
                    sb.Draw(chunk.DirtTex, dest, chunk.DirtSrc[d], Color.White);

                    // 2) se for relva, auto-tiling de bordas/interior
                    if (chunk.Grass[ly, lx])
                    {
                        bool top = (ly > 0 && chunk.Grass[ly - 1, lx]);
                        bool bottom = (ly < ChunkSize - 1 && chunk.Grass[ly + 1, lx]);
                        bool left = (lx > 0 && chunk.Grass[ly, lx - 1]);
                        bool right = (lx < ChunkSize - 1 && chunk.Grass[ly, lx + 1]);

                        int row, col;

                        if (!top)
                        {
                            // borda superior
                            row = 0;
                            col = !left ? 0
                                : !right ? 3
                                : ((lx + ly) & 1) == 0 ? 1 : 2;
                        }
                        else if (!bottom)
                        {
                            // borda inferior
                            row = 3;
                            col = !left ? 0
                                : !right ? 3
                                : ((lx + ly) & 1) == 0 ? 1 : 2;
                        }
                        else if (!left)
                        {
                            // borda esquerda
                            row = 1; col = 0;
                        }
                        else if (!right)
                        {
                            // borda direita
                            row = 1; col = 3;
                        }
                        else
                        {
                            // interior completo (usando escolha ponderada)
                            int ch = chunk.InteriorChoice[ly, lx];
                            row = (ch < 2 ? 1 : 2);
                            col = (ch % 2 == 0 ? 1 : 2);
                        }

                        sb.Draw(chunk.GrassTex, dest, chunk.GrassSrc[row, col], Color.White);
                    }
                }
        }

        /// <summary>
        /// Retorna (ou cria) o chunk em (cx,cy) e garante que já carregou os atlas.
        /// </summary>
        public Chunk GetChunk(int cx, int cy)
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

        private static int FloorDiv(int a, int b) => (a >= 0 ? a : a - b + 1) / b;
        private static int Mod(int a, int b) => ((a % b) + b) % b;

        /// <summary>
        /// Cada chunk de ChunkSize×ChunkSize, gerado via Perlin Noise determinístico.
        /// </summary>
        public class Chunk
        {
            public readonly bool[,] Grass;
            public readonly int[,] DirtIndex;
            public readonly int[,] InteriorChoice;
            public Texture2D DirtTex, GrassTex;
            public Rectangle[] DirtSrc;
            public Rectangle[,] GrassSrc;

            private static bool _atlasesLoaded;
            private static Texture2D _dAt, _gAt;
            private static Rectangle[] _dSrc;
            private static Rectangle[,] _gSrc;

            public Chunk(int cx, int cy, int size, int seed)
            {
                Grass = new bool[size, size];
                DirtIndex = new int[size, size];
                InteriorChoice = new int[size, size];

                var rnd = new Random(seed ^ cx ^ (cy << 16));

                // 1) gera dirtIndex aleatório (0..5)
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        DirtIndex[y, x] = rnd.Next(6);

                // 2) determina relva via Perlin Noise contínuo
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int gx = cx * size + x;
                        int gy = cy * size + y;
                        double n = PerlinNoise(gx * NoiseScale, gy * NoiseScale, seed);
                        Grass[y, x] = (n > Threshold);
                    }

                // 3) interiorChoice para auto-tiling (só centrais)
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        if (Grass[y, x]
                            && y > 0 && y < size - 1
                            && x > 0 && x < size - 1
                            && Grass[y - 1, x] && Grass[y + 1, x]
                            && Grass[y, x - 1] && Grass[y, x + 1])
                        {
                            int r = rnd.Next(9);
                            InteriorChoice[y, x] =
                                r < 4 ? 0 :
                                r < 7 ? 1 :
                                r < 8 ? 2 : 3;
                        }
                        else InteriorChoice[y, x] = -1;
                    }
            }

            /// <summary>
            /// Carrega atlas de dirt e grass2 apenas uma vez.
            /// </summary>
            public void LoadContent(ContentManager content)
            {
                if (!_atlasesLoaded)
                {
                    _dAt = content.Load<Texture2D>("dirt");
                    _dSrc = new Rectangle[6];
                    for (int i = 0; i < 6; i++)
                        _dSrc[i] = new Rectangle(i * 16, 0, 16, 16);

                    _gAt = content.Load<Texture2D>("grass2");
                    _gSrc = new Rectangle[4, 4];
                    for (int r = 0; r < 4; r++)
                        for (int c = 0; c < 4; c++)
                            _gSrc[r, c] = new Rectangle(c * 16, r * 16, 16, 16);

                    _atlasesLoaded = true;
                }

                DirtTex = _dAt;
                DirtSrc = _dSrc;
                GrassTex = _gAt;
                GrassSrc = _gSrc;
            }

            // --- Funções Perlin Noise 2D ---

            private static double Noise(int ix, int iy, int seed)
            {
                int n = ix + iy * 57 + seed * 131;
                n = (n << 13) ^ n;
                return 1.0 - ((n * (n * (n * 15731 + 789221) + 1376312589)
                              & 0x7fffffff) / 1073741824.0);
            }

            private static double Lerp(double a, double b, double t)
                => a + (b - a) * (0.5 - 0.5 * Math.Cos(t * Math.PI));

            private static double Smooth(double x, double y, int seed)
            {
                int ix = (int)Math.Floor(x);
                int iy = (int)Math.Floor(y);
                double fx = x - ix;
                double fy = y - iy;

                double v1 = Noise(ix, iy, seed);
                double v2 = Noise(ix + 1, iy, seed);
                double v3 = Noise(ix, iy + 1, seed);
                double v4 = Noise(ix + 1, iy + 1, seed);

                double i1 = Lerp(v1, v2, fx);
                double i2 = Lerp(v3, v4, fx);
                return Lerp(i1, i2, fy);
            }

            private static double PerlinNoise(double x, double y, int seed)
            {
                double total = 0, freq = 1, amp = 1, max = 0;
                for (int o = 0; o < Octaves; o++)
                {
                    total += Smooth(x * freq, y * freq, seed + o) * amp;
                    max += amp;
                    amp *= Persistence;
                    freq *= Lacunarity;
                }
                return total / max;
            }
        }
    }
}
