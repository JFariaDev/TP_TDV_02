using System;
using Microsoft.Xna.Framework;

namespace Bratalian2
{
    public static class MapGenerator
    {
        /// <summary>
        /// Gera um MapZone completo, com bordas, zonas jogáveis, caminhos e bushes
        /// </summary>
        public static MapZone Generate(
            int exteriorMargin, int interiorMargin,
            int zoneW, int zoneH, int pathW,
            int gap, int vgap)
        {
            int width = zoneW * 2 + gap + 2 * (exteriorMargin + 1);
            int height = zoneH * 2 + vgap + 2 * (exteriorMargin + 1);

            var zone = new MapZone(width, height);

            // 1) Relva
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    zone.Tiles[x, y] = TileType.Grass;

            // 2) Borda exterior
            for (int x = 0; x < width; x++)
            {
                zone.Tiles[x, 0] = TileType.BorderTop;
                zone.Tiles[x, height - 1] = TileType.BorderBottom;
            }
            for (int y = 0; y < height; y++)
            {
                zone.Tiles[0, y] = TileType.BorderLeft;
                zone.Tiles[width - 1, y] = TileType.BorderRight;
            }
            zone.Tiles[0, 0] = TileType.BorderTopLeft;
            zone.Tiles[width - 1, 0] = TileType.BorderTopRight;
            zone.Tiles[0, height - 1] = TileType.BorderBottomLeft;
            zone.Tiles[width - 1, height - 1] = TileType.BorderBottomRight;

            // 3) Calcula posições das 4 zonas jogáveis
            int z1x = exteriorMargin + 1, z1y = exteriorMargin + 1;
            int z2x = z1x + zoneW + gap;
            int z3y = z1y + zoneH + vgap;
            int z3x = z1x, z4x = z2x;

            // 4) Piso interno (Ground)
            FillRect(zone, z1x + interiorMargin, z1y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);
            FillRect(zone, z2x + interiorMargin, z1y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);
            FillRect(zone, z3x + interiorMargin, z3y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);
            FillRect(zone, z4x + interiorMargin, z3y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);

            // 5) Bordas das zonas
            DrawZoneBorder(zone, z1x, z1y, zoneW, zoneH, interiorMargin, pathW, "down");
            DrawZoneBorder(zone, z2x, z1y, zoneW, zoneH, interiorMargin, pathW, "down");
            DrawZoneBorder(zone, z3x, z3y, zoneW, zoneH, interiorMargin, pathW, "up");
            DrawZoneBorder(zone, z4x, z3y, zoneW, zoneH, interiorMargin, pathW, "up");

            // 6) Caminhos verticais
            int p1x = z1x + zoneW / 2, p2x = z2x + zoneW / 2;
            int pYs = z1y + zoneH - interiorMargin, pYe = z3y + interiorMargin;
            for (int y = pYs; y < pYe; y++)
                for (int px = -pathW / 2; px <= pathW / 2; px++)
                {
                    Set(zone, p1x + px, y, TileType.Ground);
                    Set(zone, p2x + px, y, TileType.Ground);
                }

            // 7) Bushes nos cantos e faixas laterais
            FillBushCornersAndSide(zone, exteriorMargin, zoneW, zoneH, gap, vgap);

            // 8) Manchas 5x5 aleatórias de bush (fora das zonas e caminhos)
            GenerateRandomBushZones5x5(zone, 12, exteriorMargin, zoneW, zoneH, gap, vgap);

            // 9) Ajuste de bordas internas e ground edges
            zone.GenerateBorders();
            zone.GenerateGroundEdges();

            return zone;
        }

        private static void FillRect(MapZone z, int x, int y, int w, int h, TileType t)
        {
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + h; j++)
                    Set(z, i, j, t);
        }

        private static void Set(MapZone z, int x, int y, TileType t)
        {
            if (x >= 0 && x < z.Width && y >= 0 && y < z.Height)
                z.Tiles[x, y] = t;
        }

        private static void DrawZoneBorder(MapZone zone, int x, int y, int w, int h, int margin, int pathW, string dir)
        {
            int center = x + w / 2;
            // topo
            for (int xx = x; xx < x + w; xx++)
                if (!(dir == "down" && Math.Abs(xx - center) <= pathW / 2))
                    Set(zone, xx, y, TileType.BorderTop);
            // base
            for (int xx = x; xx < x + w; xx++)
                if (!(dir == "up" && Math.Abs(xx - center) <= pathW / 2))
                    Set(zone, xx, y + h - 1, TileType.BorderBottom);
            // lados
            for (int yy = y; yy < y + h; yy++)
            {
                Set(zone, x, yy, TileType.BorderLeft);
                Set(zone, x + w - 1, yy, TileType.BorderRight);
            }
            // cantos
            Set(zone, x, y, TileType.BorderTopLeft);
            Set(zone, x + w - 1, y, TileType.BorderTopRight);
            Set(zone, x, y + h - 1, TileType.BorderBottomLeft);
            Set(zone, x + w - 1, y + h - 1, TileType.BorderBottomRight);
        }

        private static void FillBushCornersAndSide(MapZone zone, int m, int wZ, int hZ, int gap, int vgap)
        {
            int W = zone.Width, H = zone.Height;
            int bW = m - 1, bH = m - 1;
            // quatro cantos
            for (int x = 1; x <= bW; x++)
                for (int y = 1; y <= bH; y++)
                    if (zone.Tiles[x, y] == TileType.Grass) zone.Tiles[x, y] = TileType.Bush;
            for (int x = W - bW - 1; x < W - 1; x++)
                for (int y = 1; y <= bH; y++)
                    if (zone.Tiles[x, y] == TileType.Grass) zone.Tiles[x, y] = TileType.Bush;
            for (int x = 1; x <= bW; x++)
                for (int y = H - bH - 1; y < H - 1; y++)
                    if (zone.Tiles[x, y] == TileType.Grass) zone.Tiles[x, y] = TileType.Bush;
            for (int x = W - bW - 1; x < W - 1; x++)
                for (int y = H - bH - 1; y < H - 1; y++)
                    if (zone.Tiles[x, y] == TileType.Grass) zone.Tiles[x, y] = TileType.Bush;
            // faixas superiores laterais
            int zx = m + 1, zy = m + 1;
            for (int x = 1; x <= bW; x++)
                for (int y = bH + 1; y < zy; y++)
                    if (zone.Tiles[x, y] == TileType.Grass) zone.Tiles[x, y] = TileType.Bush;
            for (int x = W - bW - 1; x < W - 1; x++)
                for (int y = bH + 1; y < zy; y++)
                    if (zone.Tiles[x, y] == TileType.Grass) zone.Tiles[x, y] = TileType.Bush;
        }

        private static void GenerateRandomBushZones5x5(
            MapZone zone, int count,
            int m, int wZ, int hZ, int gap, int vgap)
        {
            int W = zone.Width, H = zone.Height;
            var rnd = new Random();
            // zonas jogáveis
            int z1x = m + 1, z1y = m + 1;
            int z2x = z1x + wZ + gap;
            int z3y = z1y + hZ + vgap;
            int z3x = z1x, z4x = z2x;
            var rects = new[]
            {
                new Rectangle(z1x,z1y,wZ,hZ),
                new Rectangle(z2x,z1y,wZ,hZ),
                new Rectangle(z3x,z3y,wZ,hZ),
                new Rectangle(z4x,z3y,wZ,hZ),
            };
            int p1x = z1x + wZ / 2, p2x = z2x + wZ / 2;
            int pYs = z1y + hZ - 2, pYe = z3y + 2;

            int tries = 0;
            while (count > 0 && tries < 3000)
            {
                tries++;
                int bx = rnd.Next(2, W - 7), by = rnd.Next(2, H - 7);

                // evita zonas jogáveis
                bool bad = false;
                foreach (var r in rects)
                    if (new Rectangle(bx, by, 5, 5).Intersects(r))
                    { bad = true; break; }
                if (bad) continue;

                // evita caminhos
                for (int px = -1; px <= 1; px++)
                    for (int y = 0; y < 5; y++)
                        if ((bx <= p1x + px && bx + 4 >= p1x + px && by + y >= pYs && by + y < pYe) ||
                            (bx <= p2x + px && bx + 4 >= p2x + px && by + y >= pYs && by + y < pYe))
                            bad = true;
                if (bad) continue;

                // só grass
                bool ok = true;
                for (int dx = 0; dx < 5 && ok; dx++)
                    for (int dy = 0; dy < 5; dy++)
                        if (zone.Tiles[bx + dx, by + dy] != TileType.Grass)
                            ok = false;
                if (!ok) continue;

                // planta bush ~85%
                for (int dx = 0; dx < 5; dx++)
                    for (int dy = 0; dy < 5; dy++)
                        if (rnd.NextDouble() < 0.85)
                            zone.Tiles[bx + dx, by + dy] = TileType.Bush;

                count--;
            }
        }
    }
}
