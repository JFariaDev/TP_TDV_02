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
        private const int TileSize = 16;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Texturas de mapa e fundo
        private Texture2D tilesetTex, borderTex, bushTex, battleBackgroundTex;

        // Player e mapa
        private Player player;
        private MapZone bigZone;

        // Parâmetros do mapa
        private int exteriorMargin = 12, interiorMargin = 2;
        private int zoneW = 38, zoneH = 22, pathW = 3, gap = 30, vgap = 24;

        // Câmera
        private Camera2D camera;

        // Estado de jogo
        private enum GameState { Exploring, Battle }
        private GameState currentState = GameState.Exploring;

        // Bratalians
        private List<Bratalian> bratalians = new List<Bratalian>();
        private Bratalian bratalianAtual;
        private Vector2 bratalianBattlePosition;
        private bool bratalianEntrando = false;

        // Texto de encontro
        private SpriteFont font;
        private bool mostrarTextoDeEncontro = false;
        private string textoDoBratalian = "";

        // Ginásios
        private Texture2D gymBlueTex, gymGreenTex, gymRedTex;
        private Vector2 gymBluePos, gymGreenPos, gymRedPos;
        private List<Rectangle> gymCollisionRects = new List<Rectangle>();

        // Árvores
        private Texture2D treeTex, treeBlueTex, bigTreeTex, bigTreeBlueTex;
        private List<Tree> trees = new List<Tree>();
        private List<Rectangle> treeCollisionRects = new List<Rectangle>();

        // Interação com “E”
        private Texture2D interactTex;
        private Tree promptTree;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            bigZone = MapGenerator.Generate(
                exteriorMargin, interiorMargin,
                zoneW, zoneH, pathW,
                gap, vgap);

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

            // Texturas de mapa
            tilesetTex = Content.Load<Texture2D>("tileset");
            borderTex = Content.Load<Texture2D>("borda");
            bushTex = Content.Load<Texture2D>("bush");
            battleBackgroundTex = Content.Load<Texture2D>("battle_bck");
            font = Content.Load<SpriteFont>("Arial");

            // Player
            var idleTex = Content.Load<Texture2D>("Char_002_Idle");
            var walkTex = Content.Load<Texture2D>("Char_002");
            player = new Player(idleTex, walkTex);
            int sx = exteriorMargin + 1 + interiorMargin + (zoneW - 2 * interiorMargin) / 2;
            int sy = exteriorMargin + 1 + interiorMargin + (zoneH - 2 * interiorMargin) / 2;
            player.Position = new Vector2(sx * TileSize, sy * TileSize);

            // Ginásios
            gymBlueTex = Content.Load<Texture2D>("ginasioazul");
            gymGreenTex = Content.Load<Texture2D>("ginasioverde");
            gymRedTex = Content.Load<Texture2D>("ginasiovermelho");
            int z1x = exteriorMargin + 1, z1y = exteriorMargin + 1;
            int z2x = z1x + zoneW + gap;
            int z3y = z1y + zoneH + vgap;
            int z3x = z1x, z4x = z2x;
            Func<int, int, Vector2> center = (zx, zy) => new Vector2(
                (zx + interiorMargin + (zoneW - 2 * interiorMargin) / 2) * TileSize,
                (zy + interiorMargin + (zoneH - 2 * interiorMargin) / 2) * TileSize);
            gymBluePos = center(z2x, z1y);
            gymGreenPos = center(z3x, z3y);
            gymRedPos = center(z4x, z3y);
            gymCollisionRects.Add(new Rectangle(
                (int)gymBluePos.X - gymBlueTex.Width / 2,
                (int)gymBluePos.Y - gymBlueTex.Height,
                gymBlueTex.Width, gymBlueTex.Height));
            gymCollisionRects.Add(new Rectangle(
                (int)gymGreenPos.X - gymGreenTex.Width / 2,
                (int)gymGreenPos.Y - gymGreenTex.Height,
                gymGreenTex.Width, gymGreenTex.Height));
            gymCollisionRects.Add(new Rectangle(
                (int)gymRedPos.X - gymRedTex.Width / 2,
                (int)gymRedPos.Y - gymRedTex.Height,
                gymRedTex.Width, gymRedTex.Height));

            // Árvores e ícone E
            treeTex = Content.Load<Texture2D>("minitree");
            treeBlueTex = Content.Load<Texture2D>("minitreeazul");
            bigTreeTex = Content.Load<Texture2D>("tree");
            bigTreeBlueTex = Content.Load<Texture2D>("treeazul");
            interactTex = Content.Load<Texture2D>("E");

            var rnd = new Random();
            const double treeChance = 0.05, blueRatio = 0.1;
            for (int x = 0; x < bigZone.Width; x++)
                for (int y = 0; y < bigZone.Height; y++)
                {
                    if (bigZone.Tiles[x, y] != TileType.Grass || rnd.NextDouble() >= treeChance)
                        continue;
                    if (trees.Any(t =>
                        Math.Abs((t.Position.X / TileSize) - x) <= 1 &&
                        Math.Abs((t.Position.Y / TileSize) - y) <= 1))
                        continue;

                    bool isBlue = rnd.NextDouble() < blueRatio;
                    bool isBig = rnd.NextDouble() < 0.5;
                    var tex = isBig
                        ? (isBlue ? bigTreeBlueTex : bigTreeTex)
                        : (isBlue ? treeBlueTex : treeTex);

                    var pos = new Vector2(x * TileSize, y * TileSize);
                    trees.Add(new Tree(tex, pos, isBlue));

                    // colisão de 1 tile de largura, ajustado verticalmente
                    int collW = TileSize;
                    int offX = (int)pos.X + (tex.Width - collW) / 2;
                    treeCollisionRects.Add(new Rectangle(
                        offX, (int)pos.Y - (tex.Height - TileSize),
                        collW, tex.Height));
                }

            // Bratalians
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
                bratalians.Add(new Bratalian(tex, pos, n, new Vector2(0.2f, 0.2f)));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape)) Exit();

            if (currentState == GameState.Exploring)
            {
                var old = player.Position;
                player.Update(gameTime, ks, bigZone);

                // colisão tile-a-tile (fronteiras)
                int tx = ((int)player.Position.X + player.Width / 2) / TileSize;
                int ty = ((int)player.Position.Y + player.Height / 2) / TileSize;
                if (IsBlocked(bigZone, tx, ty))
                    player.Position = old;

                // colisão ginásios e árvores
                var pr = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.Width, player.Height);
                foreach (var gr in gymCollisionRects)
                    if (pr.Intersects(gr)) { player.Position = old; break; }
                foreach (var tr in treeCollisionRects)
                    if (pr.Intersects(tr)) { player.Position = old; break; }

                // detecção de árvore azul interativa
                promptTree = trees.FirstOrDefault(t => {
                    if (!t.IsInteractive) return false;
                    var r = t.Bounds; r.Inflate(4, 4);
                    return r.Intersects(pr);
                });

                // interação E
                if (promptTree != null && ks.IsKeyDown(Keys.E))
                {
                    if (promptTree.Texture == treeBlueTex) promptTree.Texture = treeTex;
                    else if (promptTree.Texture == bigTreeBlueTex) promptTree.Texture = bigTreeTex;
                    promptTree.IsInteractive = false;
                    promptTree = null;
                }

                // encontros
                foreach (var b in bratalians)
                {
                    var br = new Rectangle((int)b.Position.X, (int)b.Position.Y, 24, 24);
                    if (pr.Intersects(br)) { StartBattle(b); break; }
                }
            }
            else if (currentState == GameState.Battle && bratalianEntrando)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                var dest = new Vector2(GraphicsDevice.Viewport.Width - 300, 300);
                var dir = dest - bratalianBattlePosition;
                if (dir.Length() > 2f) { dir.Normalize(); bratalianBattlePosition += dir * 200f * dt; }
                else bratalianEntrando = false;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Batch 1: mundo
            var view = camera.GetViewMatrix(player.Position);
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: view);

            DrawBackground();
            DrawMapZone(bigZone);

            // árvores e player
            foreach (var t in trees) t.Draw(spriteBatch);
            player.Draw(spriteBatch);

            // ginásios
            spriteBatch.Draw(gymBlueTex, gymBluePos - new Vector2(gymBlueTex.Width / 2, gymBlueTex.Height), Color.White);
            spriteBatch.Draw(gymGreenTex, gymGreenPos - new Vector2(gymGreenTex.Width / 2, gymGreenTex.Height), Color.White);
            spriteBatch.Draw(gymRedTex, gymRedPos - new Vector2(gymRedTex.Width / 2, gymRedTex.Height), Color.White);

            // texto encontro
            if (mostrarTextoDeEncontro)
            {
                var ts = font.MeasureString(textoDoBratalian);
                var tp = player.Position - new Vector2(ts.X / 2, ts.Y + 10);
                spriteBatch.DrawString(font, textoDoBratalian, tp, Color.White);
            }

            spriteBatch.End();

            // Batch 2: UI “E”
            if (promptTree != null)
            {
                spriteBatch.Begin();
                var b = promptTree.Bounds;
                var wp = new Vector2(b.Center.X, b.Top);
                var sp = Vector2.Transform(wp, view);
                spriteBatch.Draw(interactTex,
                    sp - new Vector2(interactTex.Width / 2, interactTex.Height + 4),
                    Color.White);
                spriteBatch.End();
            }

            // Batch 3: batalha
            if (currentState == GameState.Battle)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(battleBackgroundTex, GraphicsDevice.Viewport.Bounds, Color.White);
                var ts = font.MeasureString(textoDoBratalian);
                var cp = new Vector2(GraphicsDevice.Viewport.Width / 2f - ts.X / 2, GraphicsDevice.Viewport.Height - 100);
                spriteBatch.DrawString(font, textoDoBratalian, cp, Color.White);
                if (bratalianAtual != null)
                    spriteBatch.Draw(bratalianAtual.Texture, bratalianBattlePosition, null,
                                     Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
                spriteBatch.End();
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
            var grass = new Rectangle(0, 0, TileSize, TileSize);

            for (int x = 0; x < cols; x++)
                for (int y = 0; y < rows; y++)
                {
                    var pos = new Vector2((sx + x) * TileSize, (sy + y) * TileSize);
                    spriteBatch.Draw(tilesetTex, pos, grass, Color.White);
                    if (new Random((sx + x) * 73856093 ^ (sy + y) * 19349663).NextDouble() < 0.05)
                        spriteBatch.Draw(bushTex, pos, Color.White);
                }
        }

        private void DrawMapZone(MapZone zone)
        {
            var sb = spriteBatch; int TS = TileSize;
            var g0 = new Rectangle(0, 0, TS, TS);
            var g1 = new Rectangle(16, 0, TS, TS);
            var tl0 = new Rectangle(0, 16, TS, TS);
            var tl1 = new Rectangle(16, 16, TS, TS);
            var tl2 = new Rectangle(32, 16, TS, TS);
            var bl0 = new Rectangle(0, 48, TS, TS);
            var bl1 = new Rectangle(16, 48, TS, TS);
            var bl2 = new Rectangle(32, 48, TS, TS);
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
                    if (t == TileType.Bush) { sb.Draw(bushTex, pos, Color.White); continue; }
                    if (t == TileType.GroundEdgeLeft || t == TileType.GroundEdgeRight ||
                       t == TileType.GroundEdgeBottomLeft || t == TileType.GroundEdgeBottom ||
                       t == TileType.GroundEdgeBottomRight)
                        sb.Draw(tilesetTex, pos, g0, Color.White);

                    Texture2D tex = tilesetTex; Rectangle src = g0;
                    switch (t)
                    {
                        case TileType.Grass: src = g0; break;
                        case TileType.Ground: src = g1; break;
                        case TileType.GroundEdgeTopLeft: src = tl0; break;
                        case TileType.GroundEdgeTop: src = tl1; break;
                        case TileType.GroundEdgeTopRight: src = tl2; break;
                        case TileType.GroundEdgeBottomLeft: src = bl0; break;
                        case TileType.GroundEdgeBottom: src = bl1; break;
                        case TileType.GroundEdgeBottomRight: src = bl2; break;
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
                default: return false;
            }
        }

        private void StartBattle(Bratalian b)
        {
            mostrarTextoDeEncontro = true;
            textoDoBratalian = $"Voce encontrou {b.Name}! Prepare-se para a batalha!";
            currentState = GameState.Battle;
            bratalianAtual = b;
            bratalianEntrando = true;
            bratalianBattlePosition = new Vector2(GraphicsDevice.Viewport.Width + 100, 300);
        }
    }
}
