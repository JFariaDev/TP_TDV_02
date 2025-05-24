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

        // Parâmetros do mapa (AJUSTADOS!)
        private int exteriorMargin = 12;
        private int interiorMargin = 2;
        private int zoneW = 38;    // maior largura
        private int zoneH = 22;    // maior altura
        private int pathW = 3;
        private int gap = 30;      // mais afastadas horizontalmente
        private int vgap = 24;     // mais afastadas verticalmente

        private int width, height;
        private Matrix camMat;
        private Vector2 camPos;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Calcula dimensões do mapa
            width = zoneW * 2 + gap + 2 * (exteriorMargin + 1);
            height = zoneH * 2 + vgap + 2 * (exteriorMargin + 1);

            bigZone = new MapZone(width, height);

            // Preenche tudo com relva
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    bigZone.Tiles[x, y] = TileType.Grass;

            // Borda exterior
            for (int x = 0; x < width; x++)
            {
                bigZone.Tiles[x, 0] = TileType.BorderTop;
                bigZone.Tiles[x, height - 1] = TileType.BorderBottom;
            }
            for (int y = 0; y < height; y++)
            {
                bigZone.Tiles[0, y] = TileType.BorderLeft;
                bigZone.Tiles[width - 1, y] = TileType.BorderRight;
            }
            bigZone.Tiles[0, 0] = TileType.BorderTopLeft;
            bigZone.Tiles[width - 1, 0] = TileType.BorderTopRight;
            bigZone.Tiles[0, height - 1] = TileType.BorderBottomLeft;
            bigZone.Tiles[width - 1, height - 1] = TileType.BorderBottomRight;

            // ---- Zonas ----
            // 2 em cima
            int zone1_X = exteriorMargin + 1;
            int zone2_X = zone1_X + zoneW + gap;
            int zone1_Y = exteriorMargin + 1;
            // 2 em baixo
            int zone3_Y = zone1_Y + zoneH + vgap;
            int zone3_X = zone1_X;
            int zone4_X = zone2_X;

            // Preencher as 4 zonas com chão e borda
            FillRect(bigZone, zone1_X + interiorMargin, zone1_Y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);
            FillRect(bigZone, zone2_X + interiorMargin, zone1_Y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);
            FillRect(bigZone, zone3_X + interiorMargin, zone3_Y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);
            FillRect(bigZone, zone4_X + interiorMargin, zone3_Y + interiorMargin, zoneW - 2 * interiorMargin, zoneH - 2 * interiorMargin, TileType.Ground);

            DrawZoneBorder(bigZone, zone1_X, zone1_Y, zoneW, zoneH, interiorMargin, pathW, "down");
            DrawZoneBorder(bigZone, zone2_X, zone1_Y, zoneW, zoneH, interiorMargin, pathW, "down");
            DrawZoneBorder(bigZone, zone3_X, zone3_Y, zoneW, zoneH, interiorMargin, pathW, "up");
            DrawZoneBorder(bigZone, zone4_X, zone3_Y, zoneW, zoneH, interiorMargin, pathW, "up");

            // Caminhos verticais entre zonas de cima e baixo
            int passage1X = zone1_X + zoneW / 2;
            int passage2X = zone2_X + zoneW / 2;
            int passageYStart = zone1_Y + zoneH - interiorMargin;
            int passageYEnd = zone3_Y + interiorMargin;

            for (int y = passageYStart; y < passageYEnd; y++)
                for (int px = -pathW / 2; px <= pathW / 2; px++)
                {
                    SetTileSafe(bigZone, passage1X + px, y, TileType.Ground);
                    SetTileSafe(bigZone, passage2X + px, y, TileType.Ground);
                }

            bigZone.GenerateBorders();
            bigZone.GenerateGroundEdges();

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
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
            int startX = (exteriorMargin + 1) + interiorMargin + (zoneW - 2 * interiorMargin) / 2;
            int startY = (exteriorMargin + 1) + interiorMargin + (zoneH - 2 * interiorMargin) / 2;
            player.Position = new Vector2(startX * TileSize, startY * TileSize);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update(gameTime, Keyboard.GetState(), bigZone);

            var center = new Vector2(
                graphics.PreferredBackBufferWidth / 2f,
                graphics.PreferredBackBufferHeight / 2f
            );
            camPos = player.Position - center;
            camMat = Matrix.CreateTranslation(-camPos.X, -camPos.Y, 0f);

            base.Update(gameTime);
        }

        public static bool IsBlocked(MapZone zone, int x, int y)
        {
            if (x < 0 || x >= zone.Width || y < 0 || y >= zone.Height)
                return true;
            TileType t = zone.Tiles[x, y];
            return t == TileType.BorderTop
                || t == TileType.BorderBottom
                || t == TileType.BorderLeft
                || t == TileType.BorderRight
                || t == TileType.BorderTopLeft
                || t == TileType.BorderTopRight
                || t == TileType.BorderBottomLeft
                || t == TileType.BorderBottomRight;
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

            // Fundo procedural (relva + arbustos)
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

        private void FillRect(MapZone zone, int x, int y, int w, int h, TileType type)
        {
            for (int xx = x; xx < x + w; xx++)
                for (int yy = y; yy < y + h; yy++)
                    SetTileSafe(zone, xx, yy, type);
        }

        // Corrigida para não falhar as bordas laterais nunca
        private void DrawZoneBorder(MapZone zone, int x, int y, int w, int h, int margin, int pathW, string passageDir)
        {
            int passageCenter = x + w / 2;
            // Top border
            for (int xx = x; xx < x + w; xx++)
            {
                if (passageDir == "down" && Math.Abs(xx - passageCenter) <= pathW / 2)
                    continue;
                SetTileSafe(zone, xx, y, TileType.BorderTop);
            }
            // Bottom border
            for (int xx = x; xx < x + w; xx++)
            {
                if (passageDir == "up" && Math.Abs(xx - passageCenter) <= pathW / 2)
                    continue;
                SetTileSafe(zone, xx, y + h - 1, TileType.BorderBottom);
            }
            // Left border (sempre desenha)
            for (int yy = y; yy < y + h; yy++)
                SetTileSafe(zone, x, yy, TileType.BorderLeft);
            // Right border (sempre desenha)
            for (int yy = y; yy < y + h; yy++)
                SetTileSafe(zone, x + w - 1, yy, TileType.BorderRight);

            // Cantos
            SetTileSafe(zone, x, y, TileType.BorderTopLeft);
            SetTileSafe(zone, x + w - 1, y, TileType.BorderTopRight);
            SetTileSafe(zone, x, y + h - 1, TileType.BorderBottomLeft);
            SetTileSafe(zone, x + w - 1, y + h - 1, TileType.BorderBottomRight);
        }

        private void SetTileSafe(MapZone zone, int x, int y, TileType type)
        {
            if (x >= 0 && x < zone.Width && y >= 0 && y < zone.Height)
                zone.Tiles[x, y] = type;
        }
    }
}
