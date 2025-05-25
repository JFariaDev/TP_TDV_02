using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian2
{
    public class Game1 : Game
    {
        public const int TileSize = 16;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Tela
        private enum GameState { StartMenu, Exploring, Battle }
        private GameState currentState = GameState.StartMenu;

        // Logo e botão Play
        private Texture2D logoTex, uiPixel;
        private SpriteFont font;
        private Rectangle playButtonRect;

        // Mapas e texturas
        private Texture2D tilesetTex, borderTex, bushTex, battleBackgroundTex;
        private MapZone bigZone;

        // Player
        private Player player;

        // Parâmetros do mapa
        private int exteriorMargin = 12, interiorMargin = 2;
        private int zoneW = 38, zoneH = 22, pathW = 3, gap = 30, vgap = 24;

        // Câmera
        private Camera2D camera;

        // Bratalians selvagens
        private List<Bratalian> bratalians = new List<Bratalian>();

        // Batalha atual
        private Bratalian bratalianAtual;
        private Vector2 bratalianBattlePosition;
        private bool bratalianEntrando = false;

        // Texto de encontro
        private bool mostrarTextoDeEncontro = false;
        private string textoDoBratalian = "";

        // Ginásios
        private Texture2D gymBlueTex, gymGreenTex, gymRedTex;
        private Vector2 gymBluePos, gymGreenPos, gymRedPos;
        private List<Rectangle> gymCollisionRects = new List<Rectangle>();
        private List<Bratalian> gymLeaders = new List<Bratalian>();
        private int[] gymNeedBalls = { 20, 50, 80 };
        private int[] gymNeedGold = { 10, 25, 40 };
        private int? promptGymIndex = null;

        // Shop e Enfermaria
        private Texture2D shopTex, infirmaryTex;
        private Vector2 shopPos, infirmaryPos;
        private List<Rectangle> shopCollisionRects = new List<Rectangle>();

        // Árvores
        private Texture2D treeTex, treeBlueTex, bigTreeTex, bigTreeBlueTex;
        private List<Tree> trees = new List<Tree>();
        private List<Rectangle> treeCollisionRects = new List<Rectangle>();
        private Texture2D interactTex;
        private Tree promptTree;

        // UI de itens
        private Texture2D bolaAzulTex, goldScrapTex;
        private int bolaAzulCount = 0, goldScrapCount = 0;

        // Gold drops variantes
        private Texture2D[] goldDropTex = new Texture2D[3];
        private struct GoldDrop { public Vector2 Pos; public int Variant; }
        private List<GoldDrop> goldDrops = new List<GoldDrop>();
        private GoldDrop? promptGold;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            camera = new Camera2D(GraphicsDevice.Viewport);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Fonte, logo e botão
            font = Content.Load<SpriteFont>("Arial");
            logoTex = Content.Load<Texture2D>("logo");
            uiPixel = new Texture2D(GraphicsDevice, 1, 1);
            uiPixel.SetData(new[] { Color.White });
            int bw = 200, bh = 50;
            playButtonRect = new Rectangle(
                GraphicsDevice.Viewport.Width / 2 - bw / 2,
                GraphicsDevice.Viewport.Height / 2 + logoTex.Height / 2 + 20,
                bw, bh
            );

            // Mapas e itens UI
            tilesetTex = Content.Load<Texture2D>("tileset");
            borderTex = Content.Load<Texture2D>("borda");
            bushTex = Content.Load<Texture2D>("bush");
            battleBackgroundTex = Content.Load<Texture2D>("battle_bck");
            bolaAzulTex = Content.Load<Texture2D>("bolaazul");
            goldScrapTex = Content.Load<Texture2D>("goldscrap");
            goldDropTex[0] = Content.Load<Texture2D>("gold1");
            goldDropTex[1] = Content.Load<Texture2D>("gold2");
            goldDropTex[2] = Content.Load<Texture2D>("gold3");

            // Player
            var idleTex = Content.Load<Texture2D>("Char_002_Idle");
            var walkTex = Content.Load<Texture2D>("Char_002");
            player = new Player(idleTex, walkTex);
            // Posicionar no centro da zona 1
            int sx = exteriorMargin + interiorMargin + 1 + (zoneW - 2 * interiorMargin) / 2;
            int sy = exteriorMargin + interiorMargin + 1 + (zoneH - 2 * interiorMargin) / 2;
            player.Position = new Vector2(sx * TileSize, sy * TileSize);

            // Gera mapa e abre caminho no topo
            bigZone = MapGenerator.Generate(
                exteriorMargin, interiorMargin,
                zoneW, zoneH, pathW,
                gap, vgap);
            int midX = bigZone.Width / 2;
            for (int dx = -pathW / 2; dx <= pathW / 2; dx++)
                bigZone.Tiles[midX + dx, 0] = TileType.Ground;
            bigZone.GenerateGroundEdges();

            // Shop e Enfermaria
            shopTex = Content.Load<Texture2D>("shop");
            infirmaryTex = Content.Load<Texture2D>("enfermaria");
            int z1x = exteriorMargin + 1, z1y = exteriorMargin + 1, offs = 5;
            int ixL = z1x + interiorMargin + 1 + offs;
            int ixR = z1x + zoneW - interiorMargin - 2 - offs;
            int iyT = z1y + interiorMargin + 1 + offs;
            infirmaryPos = new Vector2(ixL * TileSize, iyT * TileSize);
            shopPos = new Vector2(ixR * TileSize, iyT * TileSize);
            shopCollisionRects.Add(new Rectangle((int)shopPos.X - shopTex.Width / 2, (int)shopPos.Y - shopTex.Height, shopTex.Width, shopTex.Height));
            shopCollisionRects.Add(new Rectangle((int)infirmaryPos.X - infirmaryTex.Width / 2, (int)infirmaryPos.Y - infirmaryTex.Height, infirmaryTex.Width, infirmaryTex.Height));

            // Ginásios
            gymBlueTex = Content.Load<Texture2D>("ginasioazul");
            gymGreenTex = Content.Load<Texture2D>("ginasioverde");
            gymRedTex = Content.Load<Texture2D>("ginasiovermelho");
            Func<int, int, Vector2> ctr = (zx, zy) => new Vector2(
                (zx + interiorMargin + (zoneW - 2 * interiorMargin) / 2) * TileSize,
                (zy + interiorMargin + (zoneH - 2 * interiorMargin) / 2) * TileSize
            );
            gymBluePos = ctr(z1x + zoneW + gap, z1y);
            gymGreenPos = ctr(z1x, z1y + zoneH + vgap);
            gymRedPos = ctr(z1x + zoneW + gap, z1y + zoneH + vgap);
            gymCollisionRects.AddRange(new[]{
                new Rectangle((int)gymBluePos.X - gymBlueTex.Width/2,  (int)gymBluePos.Y - gymBlueTex.Height,  gymBlueTex.Width,  gymBlueTex.Height),
                new Rectangle((int)gymGreenPos.X - gymGreenTex.Width/2,(int)gymGreenPos.Y - gymGreenTex.Height, gymGreenTex.Width, gymGreenTex.Height),
                new Rectangle((int)gymRedPos.X - gymRedTex.Width/2,    (int)gymRedPos.Y - gymRedTex.Height,    gymRedTex.Width,   gymRedTex.Height)
            });
            // Líderes de ginásio (stub)
            gymLeaders.Add(new Bratalian(Content.Load<Texture2D>("Char_002"), gymBluePos, "Líder Azul", new Vector2(1f)));
            gymLeaders.Add(new Bratalian(Content.Load<Texture2D>("Char_002"), gymRedPos, "Líder Vermelho", new Vector2(1f)));
            gymLeaders.Add(new Bratalian(Content.Load<Texture2D>("Char_002"), gymGreenPos, "Líder Verde", new Vector2(1f)));

            // Árvores e “E”
            treeTex = Content.Load<Texture2D>("minitree");
            treeBlueTex = Content.Load<Texture2D>("minitreeazul");
            bigTreeTex = Content.Load<Texture2D>("tree");
            bigTreeBlueTex = Content.Load<Texture2D>("treeazul");
            interactTex = Content.Load<Texture2D>("E");
            var rnd = new Random();

            // Geração interna de árvores
            for (int x = 0; x < bigZone.Width; x++)
                for (int y = 0; y < bigZone.Height; y++)
                {
                    if (bigZone.Tiles[x, y] != TileType.Grass || rnd.NextDouble() >= 0.05) continue;
                    if (trees.Any(t => Math.Abs(t.Position.X / TileSize - x) <= 1 && Math.Abs(t.Position.Y / TileSize - y) <= 1))
                        continue;
                    bool isBlue = rnd.NextDouble() < 0.1;
                    bool isBig = rnd.NextDouble() < 0.5;
                    var tex = isBig ? (isBlue ? bigTreeBlueTex : bigTreeTex) : (isBlue ? treeBlueTex : treeTex);
                    var pos = new Vector2(x * TileSize, y * TileSize);
                    trees.Add(new Tree(tex, pos, isBlue));
                    int w = tex.Width / 2;
                    int h = tex.Height / 3;
                    int offX = (int)pos.X + (tex.Width - w) / 2;
                    int offY = (int)pos.Y - (tex.Height - TileSize) + (tex.Height - h) + TileSize;
                    treeCollisionRects.Add(new Rectangle(offX, offY, w, h));
                }

            // Geração externa de árvores (fundo)
            int rowsBack = GraphicsDevice.Viewport.Height / TileSize + 4;
            int colsBack = GraphicsDevice.Viewport.Width / TileSize + 4;
            for (int x = -colsBack; x < bigZone.Width + colsBack; x++)
                for (int y = -rowsBack; y < 0; y++)
                {
                    if (rnd.NextDouble() >= 0.05) continue;
                    bool isBlue = rnd.NextDouble() < 0.1;
                    bool isBig = rnd.NextDouble() < 0.5;
                    var tex = isBig ? (isBlue ? bigTreeBlueTex : bigTreeTex) : (isBlue ? treeBlueTex : treeTex);
                    var pos = new Vector2(x * TileSize, y * TileSize);
                    trees.Add(new Tree(tex, pos, isBlue));
                    int w = tex.Width / 2, h = tex.Height / 3;
                    int offX = (int)pos.X + (tex.Width - w) / 2;
                    int offY = (int)pos.Y - (tex.Height - TileSize) + (tex.Height - h) + TileSize;
                    treeCollisionRects.Add(new Rectangle(offX, offY, w, h));
                }

            // Gold drops internos
            for (int i = 0; i < 30; i++)
            {
                int gx, gy;
                do
                {
                    gx = rnd.Next(exteriorMargin, bigZone.Width - exteriorMargin);
                    gy = rnd.Next(exteriorMargin, bigZone.Height - exteriorMargin);
                } while (bigZone.Tiles[gx, gy] != TileType.Grass);
                goldDrops.Add(new GoldDrop { Pos = new Vector2(gx * TileSize, gy * TileSize), Variant = rnd.Next(3) });
            }
            // Gold drops externos
            for (int i = 0; i < 30; i++)
            {
                int gx = rnd.Next(-colsBack, bigZone.Width + colsBack);
                int gy = rnd.Next(-rowsBack, 0);
                goldDrops.Add(new GoldDrop { Pos = new Vector2(gx * TileSize, gy * TileSize), Variant = rnd.Next(3) });
            }

            // Bratalians selvagens
            string[] names = {
                "Bombardini Guzzini","Bombardino Crocodilo","Boneca Ambalabu","Brr Brr Patapim",
                "Cappuccino Assassino","Frigo Cammello","La Vaca Saturno Saturnita","Lirili Larila",
                "Tralalero Tralala","Trippi Troppi Troppa Trippa","Trulimero Trulicina","Tung Tung Tung Sahur"
            };
            foreach (var n in names)
            {
                var tex = Content.Load<Texture2D>(n.ToLower().Replace(" ", "_"));
                Vector2 pos;
                do
                {
                    int bx = rnd.Next(exteriorMargin + interiorMargin + 1,
                                      bigZone.Width - exteriorMargin - interiorMargin - 1);
                    int by = rnd.Next(exteriorMargin + interiorMargin + 1,
                                      bigZone.Height - exteriorMargin - interiorMargin - 1);
                    pos = new Vector2(bx * TileSize, by * TileSize);
                } while (bigZone.Tiles[(int)(pos.X / TileSize), (int)(pos.Y / TileSize)] != TileType.Bush);
                bratalians.Add(new Bratalian(tex, pos, n, new Vector2(0.2f)));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape)) Exit();

            if (currentState == GameState.StartMenu)
            {
                var ms = Mouse.GetState();
                if ((ms.LeftButton == ButtonState.Pressed && playButtonRect.Contains(ms.Position))
                   || ks.IsKeyDown(Keys.Enter))
                {
                    currentState = GameState.Exploring;
                }
            }
            else
            {
                var old = player.Position;
                player.Update(gameTime, ks, bigZone);

                int tx = ((int)player.Position.X + player.Width / 2) / TileSize;
                int ty = ((int)player.Position.Y + player.Height / 2) / TileSize;

                // Bloqueio (interna/exterior)
                if (IsBlocked(bigZone, tx, ty)) player.Position = old;

                var pr = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.Width, player.Height);
                foreach (var r in gymCollisionRects) if (pr.Intersects(r)) { player.Position = old; break; }
                foreach (var r in shopCollisionRects) if (pr.Intersects(r)) { player.Position = old; break; }
                foreach (var r in treeCollisionRects) if (pr.Intersects(r)) { player.Position = old; break; }

                // Detecta prompts
                promptTree = trees.FirstOrDefault(t => {
                    if (!t.IsInteractive) return false;
                    var rr = t.Bounds; rr.Inflate(4, 4);
                    return rr.Intersects(pr);
                });
                promptGymIndex = null;
                for (int i = 0; i < gymCollisionRects.Count; i++)
                {
                    if (pr.Intersects(gymCollisionRects[i])
                       && bolaAzulCount >= gymNeedBalls[i]
                       && goldScrapCount >= gymNeedGold[i])
                    {
                        promptGymIndex = i; break;
                    }
                }
                promptGold = null;
                foreach (var gd in goldDrops)
                {
                    var gr = new Rectangle((int)gd.Pos.X, (int)gd.Pos.Y, TileSize, TileSize);
                    gr.Inflate(4, 4);
                    if (gr.Intersects(pr)) { promptGold = gd; break; }
                }

                // Ações
                if (ks.IsKeyDown(Keys.E))
                {
                    if (promptTree != null)
                    {
                        var t = promptTree.Pick();
                        if (t.Texture == treeBlueTex) t.Texture = treeTex;
                        else if (t.Texture == bigTreeBlueTex) t.Texture = bigTreeTex;
                        bolaAzulCount++;
                        promptTree = null;
                    }
                    else if (promptGold.HasValue)
                    {
                        goldDrops.RemoveAll(g => g.Pos == promptGold.Value.Pos);
                        goldScrapCount++;
                        promptGold = null;
                    }
                    else if (promptGymIndex.HasValue)
                    {
                        StartBattle(gymLeaders[promptGymIndex.Value]);
                        promptGymIndex = null;
                    }
                }

                // Selvagem
                foreach (var b in bratalians)
                {
                    var br = new Rectangle((int)b.Position.X, (int)b.Position.Y, 24, 24);
                    if (pr.Intersects(br)) { StartBattle(b); break; }
                }

                // Battle update
                if (currentState == GameState.Battle && bratalianEntrando)
                {
                    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    var dest = new Vector2(GraphicsDevice.Viewport.Width - 300, 300);
                    var dir = dest - bratalianBattlePosition;
                    if (dir.Length() > 2f) { dir.Normalize(); bratalianBattlePosition += dir * 200f * dt; }
                    else bratalianEntrando = false;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (currentState == GameState.StartMenu)
            {
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();
                var logoPos = new Vector2(
                    GraphicsDevice.Viewport.Width / 2 - logoTex.Width / 2,
                    GraphicsDevice.Viewport.Height / 2 - logoTex.Height / 2 - 50
                );
                spriteBatch.Draw(logoTex, logoPos, Color.White);
                spriteBatch.Draw(uiPixel, playButtonRect, Color.Gray);
                var txt = "PLAY";
                var ms = font.MeasureString(txt);
                var tp = new Vector2(
                    playButtonRect.X + playButtonRect.Width / 2 - ms.X / 2,
                    playButtonRect.Y + playButtonRect.Height / 2 - ms.Y / 2
                );
                spriteBatch.DrawString(font, txt, tp, Color.White);
                spriteBatch.End();
            }
            else
            {
                // Mundo
                var view = camera.GetViewMatrix(player.Position);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: view);
                DrawBackground();
                DrawMapZone(bigZone);

                foreach (var gd in goldDrops)
                    spriteBatch.Draw(goldDropTex[gd.Variant], gd.Pos, Color.White);

                foreach (var t in trees) t.Draw(spriteBatch);
                player.Draw(spriteBatch);

                spriteBatch.Draw(gymBlueTex, gymBluePos - new Vector2(gymBlueTex.Width / 2, gymBlueTex.Height), Color.White);
                spriteBatch.Draw(gymGreenTex, gymGreenPos - new Vector2(gymGreenTex.Width / 2, gymGreenTex.Height), Color.White);
                spriteBatch.Draw(gymRedTex, gymRedPos - new Vector2(gymRedTex.Width / 2, gymRedTex.Height), Color.White);
                spriteBatch.Draw(infirmaryTex, infirmaryPos - new Vector2(infirmaryTex.Width / 2, infirmaryTex.Height), Color.White);
                spriteBatch.Draw(shopTex, shopPos - new Vector2(shopTex.Width / 2, shopTex.Height), Color.White);

                // Contadores nos gyms
                void DrawGymCounter(Vector2 pos, int bNeed, int gNeed)
                {
                    string s = $"{bolaAzulCount}/{bNeed}  {goldScrapCount}/{gNeed}";
                    var m = font.MeasureString(s);
                    var p = pos - new Vector2(m.X / 2, gymBlueTex.Height + m.Y + 4);
                    spriteBatch.DrawString(font, s, p, Color.White);
                }
                DrawGymCounter(gymBluePos, 20, 10);
                DrawGymCounter(gymRedPos, 50, 25);
                DrawGymCounter(gymGreenPos, 80, 40);

                if (mostrarTextoDeEncontro)
                {
                    var ts = font.MeasureString(textoDoBratalian);
                    var tp = player.Position - new Vector2(ts.X / 2, ts.Y + 10);
                    spriteBatch.DrawString(font, textoDoBratalian, tp, Color.White);
                }

                spriteBatch.End();

                // Prompt “E”
                if (promptTree != null || promptGold.HasValue || promptGymIndex.HasValue)
                {
                    spriteBatch.Begin();
                    Vector2 worldPos;
                    if (promptTree != null)
                        worldPos = new Vector2(promptTree.Bounds.Center.X, promptTree.Bounds.Top);
                    else if (promptGold.HasValue)
                        worldPos = promptGold.Value.Pos;
                    else
                    {
                        int i = promptGymIndex.Value;
                        var gp = i == 0 ? gymBluePos : (i == 1 ? gymRedPos : gymGreenPos);
                        worldPos = new Vector2(gp.X, gp.Y - gymBlueTex.Height);
                    }
                    var sp = Vector2.Transform(worldPos, view);
                    spriteBatch.Draw(interactTex,
                        sp - new Vector2(interactTex.Width / 2, interactTex.Height / 2),
                        Color.White);
                    spriteBatch.End();
                }

                // UI inventário
                spriteBatch.Begin();
                float uiScale = 0.05f;
                var uiPos = new Vector2(8, 8);

                // Bola azul
                string bt = bolaAzulCount.ToString();
                var bs = font.MeasureString(bt);
                var brRect = new Rectangle((int)uiPos.X - 4, (int)uiPos.Y - 4,
                                         (int)(bolaAzulTex.Width * uiScale + 8 + bs.X),
                                         (int)(bolaAzulTex.Height * uiScale + 8));
                spriteBatch.Draw(uiPixel, brRect, Color.Black * 0.5f);
                spriteBatch.Draw(bolaAzulTex, uiPos, null, Color.White, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(font, bt,
                    uiPos + new Vector2(bolaAzulTex.Width * uiScale + 6,
                                      (bolaAzulTex.Height * uiScale - bs.Y) / 2),
                    Color.White);

                // Gold scrap
                var sp2 = uiPos + new Vector2(0, brRect.Height + 4);
                string gt = goldScrapCount.ToString();
                var gs = font.MeasureString(gt);
                var grRect2 = new Rectangle((int)sp2.X - 4, (int)sp2.Y - 4,
                                          (int)(goldScrapTex.Width * uiScale + 8 + gs.X),
                                          (int)(goldScrapTex.Height * uiScale + 8));
                spriteBatch.Draw(uiPixel, grRect2, Color.Black * 0.5f);
                spriteBatch.Draw(goldScrapTex, sp2, null, Color.White, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(font, gt,
                    sp2 + new Vector2(goldScrapTex.Width * uiScale + 6,
                                   (goldScrapTex.Height * uiScale - gs.Y) / 2),
                    Color.White);
                spriteBatch.End();

                // Batalha
                if (currentState == GameState.Battle)
                {
                    spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                    spriteBatch.Draw(battleBackgroundTex, GraphicsDevice.Viewport.Bounds, Color.White);
                    var ts = font.MeasureString(textoDoBratalian);
                    var cp = new Vector2(GraphicsDevice.Viewport.Width / 2 - ts.X / 2,
                                       GraphicsDevice.Viewport.Height - 100);
                    spriteBatch.DrawString(font, textoDoBratalian, cp, Color.White);
                    if (bratalianAtual != null)
                        spriteBatch.Draw(bratalianAtual.Texture, bratalianBattlePosition,
                                         null, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

        private void DrawBackground()
        {
            var vp = GraphicsDevice.Viewport;
            int cols = (int)(vp.Width / (TileSize * camera.Zoom)) + 3;
            int rows = (int)(vp.Height / (TileSize * camera.Zoom)) + 3;
            var inv = Matrix.Invert(camera.GetViewMatrix(player.Position));
            var tl = Vector2.Transform(Vector2.Zero, inv);
            int sx = (int)(tl.X / TileSize) - 1, sy = (int)(tl.Y / TileSize) - 1;
            var gRect = new Rectangle(0, 0, TileSize, TileSize);

            for (int tx = 0; tx < cols; tx++)
                for (int ty = 0; ty < rows; ty++)
                {
                    var pos = new Vector2((sx + tx) * TileSize, (sy + ty) * TileSize);
                    spriteBatch.Draw(tilesetTex, pos, gRect, Color.White);
                    int seed = (sx + tx) * 73856093 ^ (sy + ty) * 19349663;
                    if (new Random(seed).NextDouble() < 0.05)
                        spriteBatch.Draw(bushTex, pos, Color.White);
                }
        }

        private void DrawMapZone(MapZone zone)
        {
            var sb = spriteBatch;
            int TS = TileSize;
            var g0 = new Rectangle(0, 0, TS, TS);
            var g1 = new Rectangle(16, 0, TS, TS);
            var t0 = new Rectangle(0, 16, TS, TS);
            var t1 = new Rectangle(16, 16, TS, TS);
            var t2 = new Rectangle(32, 16, TS, TS);
            var b0 = new Rectangle(0, 48, TS, TS);
            var b1 = new Rectangle(16, 48, TS, TS);
            var b2 = new Rectangle(32, 48, TS, TS);
            var l0 = new Rectangle(0, 32, TS, TS);
            var r0 = new Rectangle(32, 32, TS, TS);
            var B0 = new Rectangle(0, 0, TS, TS);
            var B1 = new Rectangle(16, 0, TS, TS);
            var B2 = new Rectangle(32, 0, TS, TS);
            var L0 = new Rectangle(0, 16, TS, TS);
            var R0 = new Rectangle(32, 16, TS, TS);
            var B3 = new Rectangle(0, 32, TS, TS);
            var B4 = new Rectangle(16, 32, TS, TS);
            var B5 = new Rectangle(32, 32, TS, TS);

            for (int x = 0; x < zone.Width; x++)
                for (int y = 0; y < zone.Height; y++)
                {
                    var t = zone.Tiles[x, y];
                    var pos = new Vector2(x * TS, y * TS);
                    if (t == TileType.Bush)
                    {
                        sb.Draw(bushTex, pos, Color.White);
                        continue;
                    }
                    if (t == TileType.GroundEdgeLeft || t == TileType.GroundEdgeRight ||
                       t == TileType.GroundEdgeBottomLeft || t == TileType.GroundEdgeBottom ||
                       t == TileType.GroundEdgeBottomRight)
                    {
                        sb.Draw(tilesetTex, pos, g0, Color.White);
                    }
                    Texture2D tex = tilesetTex;
                    Rectangle src = g0;
                    switch (t)
                    {
                        case TileType.Grass: src = g0; break;
                        case TileType.Ground: src = g1; break;
                        case TileType.GroundEdgeTopLeft: src = t0; break;
                        case TileType.GroundEdgeTop: src = t1; break;
                        case TileType.GroundEdgeTopRight: src = t2; break;
                        case TileType.GroundEdgeBottomLeft: src = b0; break;
                        case TileType.GroundEdgeBottom: src = b1; break;
                        case TileType.GroundEdgeBottomRight: src = b2; break;
                        case TileType.GroundEdgeLeft: src = l0; break;
                        case TileType.GroundEdgeRight: src = r0; break;
                        case TileType.BorderTopLeft: tex = borderTex; src = B0; break;
                        case TileType.BorderTop: tex = borderTex; src = B1; break;
                        case TileType.BorderTopRight: tex = borderTex; src = B2; break;
                        case TileType.BorderLeft: tex = borderTex; src = L0; break;
                        case TileType.BorderRight: tex = borderTex; src = R0; break;
                        case TileType.BorderBottomLeft: tex = borderTex; src = B3; break;
                        case TileType.BorderBottom: tex = borderTex; src = B4; break;
                        case TileType.BorderBottomRight: tex = borderTex; src = B5; break;
                    }
                    sb.Draw(tex, pos, src, Color.White);
                }
        }

        public static bool IsBlocked(MapZone zone, int x, int y)
        {
            if (y < 0) return false; // área exterior livre
            if (x < 0 || x >= zone.Width || y < 0 || y >= zone.Height) return true;
            switch (zone.Tiles[x, y])
            {
                case TileType.BorderTop:
                case TileType.BorderBottom:
                case TileType.BorderLeft:
                case TileType.BorderRight:
                case TileType.BorderTopLeft:
                case TileType.BorderTopRight:
                case TileType.BorderBottomLeft:
                case TileType.BorderBottomRight:
                    return true;
                default:
                    return false;
            }
        }

        private void StartBattle(Bratalian b)
        {
            mostrarTextoDeEncontro = true;
            textoDoBratalian = $"Encontraste {b.Name}! Prepare-se para a batalha!";
            currentState = GameState.Battle;
            bratalianAtual = b;
            bratalianEntrando = true;
            bratalianBattlePosition = new Vector2(GraphicsDevice.Viewport.Width + 100, 300);
        }
    }
}
