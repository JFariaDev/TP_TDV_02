using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Bratalian2
{
    public class Game1 : Game
    {
        private const int TileSize = 16;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Texturas
        private Texture2D tilesetTex, borderTex, bushTex;
        private Texture2D idleTex, walkTex;

        // Player e mapa
        private Player player;
        private MapZone bigZone;

        // Parâmetros do mapa (idem MapGenerator)
        private int exteriorMargin = 12, interiorMargin = 2;
        private int zoneW = 38, zoneH = 22, pathW = 3;
        private int gap = 30, vgap = 24;

        // Câmera 2D
        private Camera2D camera;

        //Bratalians
        private List<Bratalian> bratalians = new List<Bratalian>();
        private SpriteFont font;
        private bool mostrarTextoDeEncontro = false;
        private string textoDoBratalian = "";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Gera o mapa usando o gerador externo
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
            // Inicializa a câmera após GraphicsDevice estar pronto
            camera = new Camera2D(GraphicsDevice.Viewport);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Carregamento de texturas
            tilesetTex = Content.Load<Texture2D>("tileset");
            borderTex = Content.Load<Texture2D>("borda");
            bushTex = Content.Load<Texture2D>("bush");
            idleTex = Content.Load<Texture2D>("Char_002_Idle");
            walkTex = Content.Load<Texture2D>("Char_002");

            // Inicializa o jogador
            player = new Player(idleTex, walkTex);
            int startX = (exteriorMargin + 1)
                       + interiorMargin
                       + (zoneW - 2 * interiorMargin) / 2;
            int startY = (exteriorMargin + 1)
                       + interiorMargin
                       + (zoneH - 2 * interiorMargin) / 2;
            player.Position = new Vector2(startX * TileSize, startY * TileSize);

            string[] names = new string[] {
                "Bombardini Guzzini", "Bombardino Crocodilo", "Boneca Ambalabu", "Brr Brr Patapim",
                "Cappuccino Assassino", "Frigo Cammello", "La Vaca Saturno Saturnita", "Lirili Larila",
                "Tralalero Tralala", "Trippi Troppi Troppa Trippa", "Trulimero Trulicina", "Tung Tung Tung Sahur"
            };

            Texture2D[] textures = new Texture2D[names.Length];
            for (int i = 0; i < names.Length; i++)
                textures[i] = Content.Load<Texture2D>($"{names[i].ToLower().Replace(" ", "_")}");

            Random rnd = new Random();
            for (int i = 0; i < textures.Length; i++)
            {
                Vector2 pos;
                int x, y;
                do
                {
                    x = rnd.Next(exteriorMargin + interiorMargin + 1, bigZone.Width - exteriorMargin - interiorMargin - 1);
                    y = rnd.Next(exteriorMargin + interiorMargin + 1, bigZone.Height - exteriorMargin - interiorMargin - 1);

                    pos = new Vector2(x * TileSize, y * TileSize);
                } while (bigZone.Tiles[x, y] != TileType.Bush);


                bratalians.Add(new Bratalian(textures[i], pos, names[i], new Vector2(0.2f, 0.2f)));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Atualiza o jogador com colisão
            player.Update(gameTime, Keyboard.GetState(), bigZone);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var view = camera.GetViewMatrix(player.Position);

            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: view);

            DrawBackground();
            DrawMapZone(bigZone);
            player.Draw(spriteBatch);

            foreach (var bratalian in bratalians)
            {
                Rectangle playerRect = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.width, player.height);
                Rectangle bratalianRect = new Rectangle((int)bratalian.Position.X, (int)bratalian.Position.Y, bratalian.width, bratalian.height);

                if (playerRect.Intersects(bratalianRect))
                {
                    StartBattle(bratalian);
                    break;
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawBackground()
        {
            // Fundo procedural (relva + arbustos)
            var vp = GraphicsDevice.Viewport;
            int cols = (int)(vp.Width / (TileSize * camera.Zoom)) + 3;
            int rows = (int)(vp.Height / (TileSize * camera.Zoom)) + 3;

            // Canto superior-esquerdo em coordenadas mundo
            var inv = Matrix.Invert(camera.GetViewMatrix(player.Position));
            var topLeftWorld = Vector2.Transform(Vector2.Zero, inv);
            int sx = (int)(topLeftWorld.X / TileSize) - 1;
            int sy = (int)(topLeftWorld.Y / TileSize) - 1;

            var grassRect = new Rectangle(0, 0, TileSize, TileSize);
            for (int tx = 0; tx < cols; tx++)
                for (int ty = 0; ty < rows; ty++)
                {
                    int wx = sx + tx, wy = sy + ty;
                    var pos = new Vector2(wx * TileSize, wy * TileSize);
                    spriteBatch.Draw(tilesetTex, pos, grassRect, Color.White);

                    int seed = wx * 73856093 ^ wy * 19349663;
                    var rnd = new System.Random(seed);
                    if (rnd.NextDouble() < 0.05)
                        spriteBatch.Draw(bushTex, pos, Color.White);
                }
        }

        private void DrawMapZone(MapZone zone)
        {
            var sb = spriteBatch;
            int TS = TileSize;

            var grass = new Rectangle(0, 0, TS, TS);
            var ground = new Rectangle(16, 0, TS, TS);
            var t_tl = new Rectangle(0, 16, TS, TS);
            var t_t = new Rectangle(16, 16, TS, TS);
            var t_tr = new Rectangle(32, 16, TS, TS);
            var b_bl = new Rectangle(0, 48, TS, TS);
            var b_b = new Rectangle(16, 48, TS, TS);
            var b_br = new Rectangle(32, 48, TS, TS);
            var s_l = new Rectangle(0, 32, TS, TS);
            var s_r = new Rectangle(32, 32, TS, TS);

            var bTL = new Rectangle(0, 0, TS, TS);
            var bT = new Rectangle(16, 0, TS, TS);
            var bTR = new Rectangle(32, 0, TS, TS);
            var bL = new Rectangle(0, 16, TS, TS);
            var bR = new Rectangle(32, 16, TS, TS);
            var bBL2 = new Rectangle(0, 32, TS, TS);
            var bB2 = new Rectangle(16, 32, TS, TS);
            var bBR2 = new Rectangle(32, 32, TS, TS);

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
                        case TileType.BorderTopLeft: tex = borderTex; src = bTL; break;
                        case TileType.BorderTop: tex = borderTex; src = bT; break;
                        case TileType.BorderTopRight: tex = borderTex; src = bTR; break;
                        case TileType.BorderLeft: tex = borderTex; src = bL; break;
                        case TileType.BorderRight: tex = borderTex; src = bR; break;
                        case TileType.BorderBottomLeft: tex = borderTex; src = bBL2; break;
                        case TileType.BorderBottom: tex = borderTex; src = bB2; break;
                        case TileType.BorderBottomRight: tex = borderTex; src = bBR2; break;
                    }
                    sb.Draw(tex, pos, src, Color.White);
                }
        }

        public static bool IsBlocked(MapZone zone, int x, int y)
        {
            if (x < 0 || x >= zone.Width || y < 0 || y >= zone.Height)
                return true;
            var t = zone.Tiles[x, y];
            switch (t)
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

        private void StartBattle(Bratalian bratalian)
        {
            mostrarTextoDeEncontro = true;
            textoDoBratalian = $"Você encontrou {bratalian.Name}! Prepare-se para a batalha!";
            // Aqui você pode alternar um flag para mudar o GameState e desenhar tela de batalha.
        }
    }
}
