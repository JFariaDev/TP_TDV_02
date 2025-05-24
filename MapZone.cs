using System;

namespace Bratalian2
{
    public enum TileType
    {
        Grass,
        Ground,
        GroundEdgeTopLeft, GroundEdgeTop, GroundEdgeTopRight,
        GroundEdgeLeft, GroundEdgeRight,
        GroundEdgeBottomLeft, GroundEdgeBottom, GroundEdgeBottomRight,
        BorderTopLeft, BorderTop, BorderTopRight,
        BorderLeft, BorderRight,
        BorderBottomLeft, BorderBottom, BorderBottomRight,
        Bush
    }

    public class MapZone
    {
        public int Width, Height;
        public TileType[,] Tiles;

        public MapZone(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileType[width, height];
        }

        public void GenerateBorders()
        {
            for (int x = 0; x < Width; x++)
            {
                Tiles[x, 0] = TileType.BorderTop;
                Tiles[x, Height - 1] = TileType.BorderBottom;
            }
            for (int y = 0; y < Height; y++)
            {
                Tiles[0, y] = TileType.BorderLeft;
                Tiles[Width - 1, y] = TileType.BorderRight;
            }
            Tiles[0, 0] = TileType.BorderTopLeft;
            Tiles[Width - 1, 0] = TileType.BorderTopRight;
            Tiles[0, Height - 1] = TileType.BorderBottomLeft;
            Tiles[Width - 1, Height - 1] = TileType.BorderBottomRight;
        }

        public void GenerateGroundEdges()
        {
            for (int x = 1; x < Width - 1; x++)
            {
                for (int y = 1; y < Height - 1; y++)
                {
                    if (Tiles[x, y] != TileType.Grass) continue;
                    // cima
                    if (Tiles[x, y + 1] == TileType.Ground)
                        Tiles[x, y] = TileType.GroundEdgeTop;
                    // baixo
                    else if (Tiles[x, y - 1] == TileType.Ground)
                        Tiles[x, y] = TileType.GroundEdgeBottom;
                    // direita
                    else if (Tiles[x + 1, y] == TileType.Ground)
                        Tiles[x, y] = TileType.GroundEdgeLeft;
                    // esquerda
                    else if (Tiles[x - 1, y] == TileType.Ground)
                        Tiles[x, y] = TileType.GroundEdgeRight;
                }
            }
        }
    }
}
