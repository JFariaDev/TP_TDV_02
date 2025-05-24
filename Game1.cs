using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian2
{
    public class Game1 : Game
    {
        private const int TileSize = 16;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Texture2D tilesetTex, borderTex, bushTex;
        private Texture2D idleTex, walkTex;
        private Player player;
        private MapZone bigZone;

        private int gap, zoneW, zoneH, pathW, nZones, width, height;
        private Matrix camMat;
        private Vector2 camPos;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // ---- Parâmetros configuráveis ----
            zoneW = 28;   // largura da zona
            zoneH = 20;   // altura da zona
            pathW = 3;    // largura do caminho
            gap = 12;     // separação entre zonas
            nZones = 3;   // número de zonas

            width = nZones * zoneW + (nZones - 1) * gap + gap;
            height = zoneH + 2 * gap;
            bigZone = new MapZone(width, height);

            // Preenche tudo com relva
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    bigZone.Tiles[x, y] = TileType.Grass;

            int currentX = gap;

            for (int i = 0; i < nZones; i++)
            {
                bool hasLeftPath = i > 0;
                bool hasRightPath = i < nZones - 1;

                // Desenha a zona toda a chão (Ground)
                for (int x = currentX; x < currentX + zoneW; x++)
                    for (int y = gap; y < gap + zoneH; y++)
                        bigZone.Tiles[x, y] = TileType.Ground;

                // Aplica bordas à zona (paredes de pedra à volta) - com proteção!
                for (int x = currentX - 1; x <= currentX + zoneW; x++)
                {
                    // Topo e base
                    if (x >= 0 && x < bigZone.Width)
                    {
                        if (gap - 1 >= 0)
                            bigZone.Tiles[x, gap - 1] = TileType.BorderTop;
                        if (gap + zoneH < bigZone.Height)
                            bigZone.Tiles[x, gap + zoneH] = TileType.BorderBottom;
                    }
                }
                for (int y = gap - 1; y <= gap + zoneH; y++)
                {
                    if (y >= 0 && y < bigZone.Height)
                    {
                        if (currentX - 1 >= 0)
                            bigZone.Tiles[currentX - 1, y] = TileType.BorderLeft;
                        if (currentX + zoneW < bigZone.Width)
                            bigZone.Tiles[currentX + zoneW, y] = TileType.BorderRight;
                    }
                }
                // CANTOS
                if (currentX - 1 >= 0 && gap - 1 >= 0)
                    bigZone.Tiles[currentX - 1, gap - 1] = TileType.BorderTopLeft;
                if (currentX + zoneW < bigZone.Width && gap - 1 >= 0)
                    bigZone.Tiles[currentX + zoneW, gap - 1] = TileType.BorderTopRight;
                if (currentX - 1 >= 0 && gap + zoneH < bigZone.Height)
                    bigZone.Tiles[currentX - 1, gap + zoneH] = TileType.BorderBottomLeft;
                if (currentX + zoneW < bigZone.Width && gap + zoneH < bigZone.Height)
                    bigZone.Tiles[currentX + zoneW, gap + zoneH] = TileType.BorderBottomRight;

                // Faz o corte da borda nas passagens (caminho horizontal)
                if (hasRightPath)
                {
                    int zoneCenterY = gap + zoneH / 2;
                    int pathStartY = zoneCenterY - pathW / 2;
                    int cutStartY = pathStartY - 2;
                    int cutEndY = pathStartY + pathW + 2;

                    for (int y = cutStartY; y < cutEndY; y++)
                    {
                        if (y >= 0 && y < bigZone.Height && currentX + zoneW < bigZone.Width)
                        {
                            // Corta a borda direita
                            bigZone.Tiles[currentX + zoneW, y] = TileType.Ground;
                        }
                    }

                    // Cria o caminho horizontal (chão)
                    int caminhoXini = currentX + zoneW;
                    int caminhoXfim = caminhoXini + gap;
                    for (int x = caminhoXini; x < caminhoXfim; x++)
                        for (int y = pathStartY; y < pathStartY + pathW; y++)
                            if (x >= 0 && x < bigZone.Width && y >= 0 && y < bigZone.Height)
                                bigZone.Tiles[x, y] = TileType.Ground;
                }

                if (hasLeftPath)
                {
                    int zoneCenterY = gap + zoneH / 2;
                    int pathStartY = zoneCenterY - pathW / 2;
                    int cutStartY = pathStartY - 2;
                    int cutEndY = pathStartY + pathW + 2;

                    for (int y = cutStartY; y < cutEndY; y++)
                    {
                        if (y >= 0 && y < bigZone.Height && currentX - 1 >= 0)
                        {
                            // Corta a borda esquerda
                            bigZone.Tiles[currentX - 1, y] = TileType.Ground;
                        }
                    }
                }

                currentX += zoneW + gap;
            }

            bigZone.GenerateBorders();
            bigZone.GenerateGroundEdges();

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();
        }

        protected override void Initialize() { base.Initialize(); }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tilesetTex = Content.Load<Texture2D>("tileset");
            borderTex = Content.Load<Texture2D>("borda");
            bushTex = Content.Load<Texture2D>("bush");
            idleTex = Content.Load<Texture2D>("Char_002_Idle");
            walkTex = Content.Load<Texture2D>("Char_002");
            player = new Player(idleTex, walkTex);

            // Spawn centro da primeira zona
            int centerX = (gap + zoneW / 2) * TileSize;
            int centerY = (gap + zoneH / 2) * TileSize;
            player.Position = new Vector2(centerX, centerY);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update(gameTime, Keyboard.GetState());

            var center = new Vector2(
                graphics.PreferredBackBufferWidth / 2f,
                graphics.PreferredBackBufferHeight / 2f
            );
            camPos = player.Position - center;
            camMat = Matrix.CreateTranslation(-camPos.X, -camPos.Y, 0f);

            base.Update(gameTime);
        }

        private void DrawMapZone(MapZone zone)
        {
            Rectangle grass = new Rectangle(0, 0, 16, 16);
            Rectangle ground = new Rectangle(16, 0, 16, 16);
            Rectangle tl_up = new Rectangle(0, 16, 16, 16);
            Rectangle mid_up = new Rectangle(16, 16, 16, 16);
            Rectangle tr_up = new Rectangle(32, 16, 16, 16);
            Rectangle bl_dn = new Rectangle(0, 48, 16, 16);
            Rectangle mid_dn = new Rectangle(16, 48, 16, 16);
            Rectangle br_dn = new Rectangle(32, 48, 16, 16);
            Rectangle sideLeftSrc = new Rectangle(0, 32, 16, 16);
            Rectangle sideRightSrc = new Rectangle(32, 32, 16, 16);
            Rectangle b_tl = new Rectangle(0, 0, 16, 16);
            Rectangle b_t = new Rectangle(16, 0, 16, 16);
            Rectangle b_tr = new Rectangle(32, 0, 16, 16);
            Rectangle b_l = new Rectangle(0, 16, 16, 16);
            Rectangle b_r = new Rectangle(32, 16, 16, 16);
            Rectangle b_bl = new Rectangle(0, 32, 16, 16);
            Rectangle b_b = new Rectangle(16, 32, 16, 16);
            Rectangle b_br = new Rectangle(32, 32, 16, 16);

            for (int x = 0; x < zone.Width; x++)
                for (int y = 0; y < zone.Height; y++)
                {
                    var t = zone.Tiles[x, y];
                    var pos = new Vector2(x * TileSize, y * TileSize);

                    if (t == TileType.GroundEdgeLeft || t == TileType.GroundEdgeRight ||
                        t == TileType.GroundEdgeBottomLeft || t == TileType.GroundEdgeBottom ||
                        t == TileType.GroundEdgeBottomRight)
                        spriteBatch.Draw(tilesetTex, pos, grass, Color.White);

                    Texture2D tex = tilesetTex;
                    Rectangle src = grass;
                    SpriteEffects flip = SpriteEffects.None;
                    switch (t)
                    {
                        case TileType.Grass: src = grass; break;
                        case TileType.Ground: src = ground; break;
                        case TileType.GroundEdgeTopLeft: src = tl_up; break;
                        case TileType.GroundEdgeTop: src = mid_up; break;
                        case TileType.GroundEdgeTopRight: src = tr_up; break;
                        case TileType.GroundEdgeBottomLeft: src = bl_dn; break;
                        case TileType.GroundEdgeBottom: src = mid_dn; break;
                        case TileType.GroundEdgeBottomRight: src = br_dn; break;
                        case TileType.GroundEdgeLeft: src = sideLeftSrc; break;
                        case TileType.GroundEdgeRight: src = sideRightSrc; break;
                        case TileType.BorderTopLeft: tex = borderTex; src = b_tl; break;
                        case TileType.BorderTop: tex = borderTex; src = b_t; break;
                        case TileType.BorderTopRight: tex = borderTex; src = b_tr; break;
                        case TileType.BorderLeft: tex = borderTex; src = b_l; break;
                        case TileType.BorderRight: tex = borderTex; src = b_r; break;
                        case TileType.BorderBottomLeft: tex = borderTex; src = b_bl; break;
                        case TileType.BorderBottom: tex = borderTex; src = b_b; break;
                        case TileType.BorderBottomRight: tex = borderTex; src = b_br; break;
                    }
                    spriteBatch.Draw(tex, pos, src, Color.White, 0f, Vector2.Zero, 1f, flip, 0f);
                }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: camMat
            );

            int vw = graphics.PreferredBackBufferWidth,
                vh = graphics.PreferredBackBufferHeight;
            int sx = (int)Math.Floor(camPos.X / TileSize) - 1,
                sy = (int)Math.Floor(camPos.Y / TileSize) - 1;
            int cols = vw / TileSize + 3,
                rows = vh / TileSize + 3;
            Rectangle grass = new Rectangle(0, 0, 16, 16);
            for (int tx = 0; tx < cols; tx++)
                for (int ty = 0; ty < rows; ty++)
                {
                    int wx = sx + tx, wy = sy + ty;
                    var wp = new Vector2(wx * TileSize, wy * TileSize);
                    spriteBatch.Draw(tilesetTex, wp, grass, Color.White);
                    int seed = wx * 73856093 ^ wy * 19349663;
                    var rnd = new Random(seed);
                    if (rnd.NextDouble() < 0.05)
                        spriteBatch.Draw(bushTex, wp, Color.White);
                }

            DrawMapZone(bigZone);
            player.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
