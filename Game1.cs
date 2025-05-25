using System;
using System.Collections.Generic;
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
        private Texture2D tilesetTex;
        private Texture2D borderTex;
        private Texture2D bushTex;
        private Texture2D battleBackgroundTex;

        // Player e mapa
        private Player player;
        private MapZone bigZone;

        // Parâmetros do mapa
        private int exteriorMargin = 12;
        private int interiorMargin = 2;
        private int zoneW = 38;
        private int zoneH = 22;
        private int pathW = 3;
        private int gap = 30;
        private int vgap = 24;

        // Câmera 2D
        private Camera2D camera;

        // Bratalians e batalha
        private List<Bratalian> bratalians = new List<Bratalian>();
        private Bratalian bratalianAtual;
        private Vector2 bratalianBattlePosition;
        private bool bratalianEntrando = false;

        // Texto de encontro
        private SpriteFont font;
        private bool mostrarTextoDeEncontro = false;
        private string textoDoBratalian = "";

        // Ginásios
        private Texture2D gymBlueTex;
        private Texture2D gymGreenTex;
        private Texture2D gymRedTex;
        private Vector2 gymBluePos;
        private Vector2 gymGreenPos;
        private Vector2 gymRedPos;
        private List<Rectangle> gymCollisionRects = new List<Rectangle>();

        // Árvores
        private Texture2D treeTex;
        private Texture2D treeBlueTex;
        private Texture2D bigTreeTex;
        private Texture2D bigTreeBlueTex;
        private List<Tree> trees = new List<Tree>();
        private List<Rectangle> treeCollisionRects = new List<Rectangle>();

        // Sprites do player
        private Texture2D idleTex;
        private Texture2D walkTex;

        private enum GameState { Exploring, Battle }
        private GameState currentState = GameState.Exploring;

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
            font = Content.Load<SpriteFont>("Arial");

            // Carrega texturas do mapa
            tilesetTex = Content.Load<Texture2D>("tileset");
            borderTex = Content.Load<Texture2D>("borda");
            bushTex = Content.Load<Texture2D>("bush");
            battleBackgroundTex = Content.Load<Texture2D>("battle_bck");

            // Player
            idleTex = Content.Load<Texture2D>("Char_002_Idle");
            walkTex = Content.Load<Texture2D>("Char_002");
            player = new Player(idleTex, walkTex);

            // Spawn player no centro da zona 1
            int startX = exteriorMargin + 1 + interiorMargin + (zoneW - 2 * interiorMargin) / 2;
            int startY = exteriorMargin + 1 + interiorMargin + (zoneH - 2 * interiorMargin) / 2;
            player.Position = new Vector2(startX * TileSize, startY * TileSize);

            // Ginásios
            gymBlueTex = Content.Load<Texture2D>("ginasioazul");
            gymGreenTex = Content.Load<Texture2D>("ginasioverde");
            gymRedTex = Content.Load<Texture2D>("ginasiovermelho");

            int z1x = exteriorMargin + 1;
            int z1y = exteriorMargin + 1;
            int z2x = z1x + zoneW + gap;
            int z3y = z1y + zoneH + vgap;
            int z3x = z1x;
            int z4x = z2x;

            Func<int, int, Vector2> center = (zx, zy) =>
                new Vector2(
                    (zx + interiorMargin + (zoneW - 2 * interiorMargin) / 2) * TileSize,
                    (zy + interiorMargin + (zoneH - 2 * interiorMargin) / 2) * TileSize
                );

            gymBluePos = center(z2x, z1y);
            gymGreenPos = center(z3x, z3y);
            gymRedPos = center(z4x, z3y);

            gymCollisionRects.Add(new Rectangle(
                (int)gymBluePos.X - gymBlueTex.Width / 2,
                (int)gymBluePos.Y - gymBlueTex.Height,
                gymBlueTex.Width,
                gymBlueTex.Height));
            gymCollisionRects.Add(new Rectangle(
                (int)gymGreenPos.X - gymGreenTex.Width / 2,
                (int)gymGreenPos.Y - gymGreenTex.Height,
                gymGreenTex.Width,
                gymGreenTex.Height));
            gymCollisionRects.Add(new Rectangle(
                (int)gymRedPos.X - gymRedTex.Width / 2,
                (int)gymRedPos.Y - gymRedTex.Height,
                gymRedTex.Width,
                gymRedTex.Height));

            // Árvores
            treeTex = Content.Load<Texture2D>("minitree");
            treeBlueTex = Content.Load<Texture2D>("minitreeazul");
            bigTreeTex = Content.Load<Texture2D>("tree");
            bigTreeBlueTex = Content.Load<Texture2D>("treeazul");

            var rnd = new Random();
            const double treeChance = 0.05;
            const double blueRatio = 0.1;

            for (int x = 0; x < bigZone.Width; x++)
            {
                for (int y = 0; y < bigZone.Height; y++)
                {
                    if (bigZone.Tiles[x, y] == TileType.Grass && rnd.NextDouble() < treeChance)
                    {
                        // garante mínimo de 1 tile de distância entre árvores
                        bool tooClose = false;
                        foreach (var t in trees)
                        {
                            int tx = (int)(t.Position.X / TileSize);
                            int ty = (int)(t.Position.Y / TileSize);
                            if (Math.Abs(tx - x) <= 1 && Math.Abs(ty - y) <= 1)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        if (tooClose) continue;

                        bool isBlue = rnd.NextDouble() < blueRatio;
                        bool isBig = rnd.NextDouble() < 0.5;
                        Texture2D tex = isBig
                            ? (isBlue ? bigTreeBlueTex : bigTreeTex)
                            : (isBlue ? treeBlueTex : treeTex);

                        Vector2 pos = new Vector2(x * TileSize, y * TileSize);
                        trees.Add(new Tree(tex, pos, false));

                        // Colisão: 1 tile de largura, altura total do sprite
                        int collW = TileSize;
                        int offsetX = (int)pos.X + (tex.Width - collW) / 2;
                        treeCollisionRects.Add(new Rectangle(
                            offsetX,
                            (int)pos.Y,
                            collW,
                            tex.Height));
                    }
                }
            }

            // Bratalians
            string[] names = {
                "Bombardini Guzzini","Bombardino Crocodilo","Boneca Ambalabu","Brr Brr Patapim",
                "Cappuccino Assassino","Frigo Cammello","La Vaca Saturno Saturnita","Lirili Larila",
                "Tralalero Tralala","Trippi Troppi Troppa Trippa","Trulimero Trulicina","Tung Tung Tung Sahur"
            };
            foreach (var n in names)
            {
                Texture2D tex = Content.Load<Texture2D>(n.ToLower().Replace(" ", "_"));
                Vector2 pos;
                do
                {
                    int bx = rnd.Next(
                        exteriorMargin + interiorMargin + 1,
                        bigZone.Width - exteriorMargin - interiorMargin - 1);
                    int by = rnd.Next(
                        exteriorMargin + interiorMargin + 1,
                        bigZone.Height - exteriorMargin - interiorMargin - 1);
                    pos = new Vector2(bx * TileSize, by * TileSize);
                }
                while (bigZone.Tiles[(int)(pos.X / TileSize), (int)(pos.Y / TileSize)] != TileType.Bush);

                bratalians.Add(new Bratalian(tex, pos, n, new Vector2(0.2f, 0.2f)));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape))
                Exit();

            if (currentState == GameState.Exploring)
            {
                Vector2 old = player.Position;
                player.Update(gameTime, ks, bigZone);

                // Colisão com ginásios
                Rectangle pr = new Rectangle(
                    (int)player.Position.X,
                    (int)player.Position.Y,
                    24, 24);
                foreach (Rectangle gr in gymCollisionRects)
                {
                    if (pr.Intersects(gr))
                    {
                        player.Position = old;
                        break;
                    }
                }

                // Colisão com árvores
                foreach (Rectangle tr in treeCollisionRects)
                {
                    if (pr.Intersects(tr))
                    {
                        player.Position = old;
                        break;
                    }
                }

                // Encontros com bratalians
                foreach (Bratalian b in bratalians)
                {
                    Rectangle br = new Rectangle(
                        (int)b.Position.X,
                        (int)b.Position.Y,
                        24, 24);
                    if (pr.Intersects(br))
                    {
                        StartBattle(b);
                        break;
                    }
                }
            }
            else if (currentState == GameState.Battle && bratalianEntrando)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Vector2 destination = new Vector2(
                    GraphicsDevice.Viewport.Width - 300, 300);
                Vector2 dir = destination - bratalianBattlePosition;
                if (dir.Length() > 2f)
                {
                    dir.Normalize();
                    bratalianBattlePosition += dir * 200f * dt;
                }
                else
                {
                    bratalianEntrando = false;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Matrix view = camera.GetViewMatrix(player.Position);
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: view);

            DrawBackground();
            DrawMapZone(bigZone);

            if (currentState == GameState.Exploring)
            {
                player.Draw(spriteBatch);

                // Ginásios
                spriteBatch.Draw(
                    gymBlueTex,
                    gymBluePos - new Vector2(gymBlueTex.Width / 2, gymBlueTex.Height),
                    Color.White);
                spriteBatch.Draw(
                    gymGreenTex,
                    gymGreenPos - new Vector2(gymGreenTex.Width / 2, gymGreenTex.Height),
                    Color.White);
                spriteBatch.Draw(
                    gymRedTex,
                    gymRedPos - new Vector2(gymRedTex.Width / 2, gymRedTex.Height),
                    Color.White);

                // Árvores
                foreach (Tree t in trees)
                    t.Draw(spriteBatch);

                // Texto de encontro
                if (mostrarTextoDeEncontro)
                {
                    Vector2 ts = font.MeasureString(textoDoBratalian);
                    Vector2 textPos = player.Position - new Vector2(ts.X / 2, ts.Y + 10);
                    spriteBatch.DrawString(font, textoDoBratalian, textPos, Color.White);
                }
            }

            spriteBatch.End();

            if (currentState == GameState.Battle)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(
                    battleBackgroundTex,
                    GraphicsDevice.Viewport.Bounds,
                    Color.White);

                Vector2 ts = font.MeasureString(textoDoBratalian);
                Vector2 center = new Vector2(
                    GraphicsDevice.Viewport.Width / 2f - ts.X / 2,
                    GraphicsDevice.Viewport.Height - 100);
                spriteBatch.DrawString(font, textoDoBratalian, center, Color.White);

                if (bratalianAtual != null)
                {
                    spriteBatch.Draw(
                        bratalianAtual.Texture,
                        bratalianBattlePosition,
                        null, Color.White,
                        0f, Vector2.Zero,
                        3f, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
            }

            base.Draw(gameTime);
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

        private void DrawBackground()
        {
            Viewport vp = GraphicsDevice.Viewport;
            int cols = (int)(vp.Width / (TileSize * camera.Zoom)) + 3;
            int rows = (int)(vp.Height / (TileSize * camera.Zoom)) + 3;
            Matrix inv = Matrix.Invert(camera.GetViewMatrix(player.Position));
            Vector2 tl = Vector2.Transform(Vector2.Zero, inv);
            int sx = (int)(tl.X / TileSize) - 1;
            int sy = (int)(tl.Y / TileSize) - 1;
            Rectangle grass = new Rectangle(0, 0, TileSize, TileSize);

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector2 pos = new Vector2((sx + x) * TileSize, (sy + y) * TileSize);
                    spriteBatch.Draw(tilesetTex, pos, grass, Color.White);
                    int seed = (sx + x) * 73856093 ^ (sy + y) * 19349663;
                    if (new Random(seed).NextDouble() < 0.05)
                        spriteBatch.Draw(bushTex, pos, Color.White);
                }
            }
        }

        private void DrawMapZone(MapZone zone)
        {
            SpriteBatch sb = spriteBatch;
            int TS = TileSize;

            Rectangle grass = new Rectangle(0, 0, TS, TS);
            Rectangle ground = new Rectangle(16, 0, TS, TS);
            Rectangle t_tl = new Rectangle(0, 16, TS, TS);
            Rectangle t_t = new Rectangle(16, 16, TS, TS);
            Rectangle t_tr = new Rectangle(32, 16, TS, TS);
            Rectangle b_bl = new Rectangle(0, 48, TS, TS);
            Rectangle b_b = new Rectangle(16, 48, TS, TS);
            Rectangle b_br = new Rectangle(32, 48, TS, TS);
            Rectangle s_l = new Rectangle(0, 32, TS, TS);
            Rectangle s_r = new Rectangle(32, 32, TS, TS);

            Rectangle Btl = new Rectangle(0, 0, TS, TS);
            Rectangle Bt = new Rectangle(16, 0, TS, TS);
            Rectangle Btr = new Rectangle(32, 0, TS, TS);
            Rectangle Bl = new Rectangle(0, 16, TS, TS);
            Rectangle Br = new Rectangle(32, 16, TS, TS);
            Rectangle Bbl = new Rectangle(0, 32, TS, TS);
            Rectangle Bb = new Rectangle(16, 32, TS, TS);
            Rectangle Bbr = new Rectangle(32, 32, TS, TS);

            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    TileType t = zone.Tiles[x, y];
                    Vector2 pos = new Vector2(x * TS, y * TS);

                    if (t == TileType.Bush)
                    {
                        sb.Draw(bushTex, pos, Color.White);
                        continue;
                    }

                    if (t == TileType.GroundEdgeLeft ||
                        t == TileType.GroundEdgeRight ||
                        t == TileType.GroundEdgeBottomLeft ||
                        t == TileType.GroundEdgeBottom ||
                        t == TileType.GroundEdgeBottomRight)
                    {
                        sb.Draw(tilesetTex, pos, grass, Color.White);
                    }

                    Texture2D tex = tilesetTex;
                    Rectangle src = grass;

                    switch (t)
                    {
                        case TileType.Grass: src = grass; break;
                        case TileType.Ground: src = ground; break;
                        case TileType.GroundEdgeTopLeft: src = t_tl; break;
                        case TileType.GroundEdgeTop: src = t_t; break;
                        case TileType.GroundEdgeTopRight: src = t_tr; break;
                        case TileType.GroundEdgeBottomLeft: src = b_bl; break;
                        case TileType.GroundEdgeBottom: src = b_b; break;
                        case TileType.GroundEdgeBottomRight: src = b_br; break;
                        case TileType.GroundEdgeLeft: src = s_l; break;
                        case TileType.GroundEdgeRight: src = s_r; break;
                        case TileType.BorderTopLeft: tex = borderTex; src = Btl; break;
                        case TileType.BorderTop: tex = borderTex; src = Bt; break;
                        case TileType.BorderTopRight: tex = borderTex; src = Btr; break;
                        case TileType.BorderLeft: tex = borderTex; src = Bl; break;
                        case TileType.BorderRight: tex = borderTex; src = Br; break;
                        case TileType.BorderBottomLeft: tex = borderTex; src = Bbl; break;
                        case TileType.BorderBottom: tex = borderTex; src = Bb; break;
                        case TileType.BorderBottomRight: tex = borderTex; src = Bbr; break;
                    }

                    sb.Draw(tex, pos, src, Color.White);
                }
            }
        }
    }
}
