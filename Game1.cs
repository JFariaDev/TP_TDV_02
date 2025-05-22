// Game1.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Map _map;
        private Player _player;

        // zoom da câmara
        private const float CameraZoom = 1.4f;

        // semente fixa para reprodução
        private const int MapSeed = 12345;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // 1) dimensões do mapa
            int rows = 60, cols = 100;
            bool[,] sampleGrass = new bool[rows, cols];

            // 2) preenche TODO o mapa com relva
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    sampleGrass[y, x] = true;

            // 3) esculpe alguns caminhos de "chão" (dirt) – sempre rectos
            var rnd = new Random(MapSeed);
            // gera entre 2 e 4 caminhos horizontais
            int hPaths = rnd.Next(2, 5);
            for (int i = 0; i < hPaths; i++)
            {
                int y = rnd.Next(1, rows - 1);
                for (int x = 0; x < cols; x++)
                    sampleGrass[y, x] = false;  // sem relva → mostra tile de dirt
            }
            // gera entre 2 e 4 caminhos verticais
            int vPaths = rnd.Next(2, 5);
            for (int i = 0; i < vPaths; i++)
            {
                int x = rnd.Next(1, cols - 1);
                for (int y = 0; y < rows; y++)
                    sampleGrass[y, x] = false;
            }

            // 4) instancia e carrega o Map
            _map = new Map(sampleGrass);
            _map.LoadContent(Content);

            // 5) spawn do player no centro
            Vector2 spawn = new Vector2(cols / 2f * Map.TileSize, rows / 2f * Map.TileSize);
            _player = new Player(spawn, speed: 120f);
            _player.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _player.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // configura a câmara com zoom, centra no player
            Viewport vp = GraphicsDevice.Viewport;
            Vector2 center = new Vector2(vp.Width, vp.Height) * 0.5f;
            Matrix cam = Matrix.CreateTranslation(new Vector3(-_player.Position, 0f))
                       * Matrix.CreateScale(CameraZoom, CameraZoom, 1f)
                       * Matrix.CreateTranslation(new Vector3(center, 0f));

            _spriteBatch.Begin(transformMatrix: cam);
            _map.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
