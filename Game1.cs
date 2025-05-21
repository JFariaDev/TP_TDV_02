using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player _player;
        private Map _map;
        private Camera2D _camera;

        private Texture2D _tilesetTex, _playerTex;
        private SpriteFont _font;

        private int[,] _mapData;
        private int _tileSize = 12;
        private HashSet<int> _impassables = new HashSet<int>
        {
            3  // obstáculo no chão bloqueia
            // (adicione tipos extra se houver outros obstáculos)
        };

        private enum GameState { Overworld, Battle }
        private GameState _state = GameState.Overworld;
        private Random _rng = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            int cols = _graphics.PreferredBackBufferWidth / _tileSize; // ≃66
            int rows = _graphics.PreferredBackBufferHeight / _tileSize; // 50
            _mapData = new int[rows, cols];

            // Preenche tudo como floor=1
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    _mapData[y, x] = 1;

            // Bordas de relva (TILE2) no interior – opcional:
            for (int x = 1; x < cols - 1; x++)
            {
                _mapData[0, x] = 2; // topo
                _mapData[rows - 1, x] = 2; // fundo
            }
            for (int y = 1; y < rows - 1; y++)
            {
                _mapData[y, 0] = 2; // lateral esquerda
                _mapData[y, cols - 1] = 2; // lateral direita
            }

            // Cantos superiores TÊM que ser TILE4 e TILE5
            _mapData[0, cols - 1] = 4; // TR
            _mapData[0, 0] = 5; // TL

            // Bordas “entre cantos” usam TILE6
            for (int x = 1; x < cols - 1; x++)
            {
                _mapData[0, x] = 6; // topo
                _mapData[rows - 1, x] = 6; // fundo
            }
            for (int y = 1; y < rows - 1; y++)
            {
                _mapData[y, 0] = 6; // esquerda
                _mapData[y, cols - 1] = 6; // direita
            }

            base.Initialize();
            _camera = new Camera2D(GraphicsDevice.Viewport);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _tilesetTex = Content.Load<Texture2D>("tileset");
            _playerTex = Content.Load<Texture2D>("player");
            _font = Content.Load<SpriteFont>("File");

            _map = new Map(_tilesetTex, _mapData, _tileSize);

            // player no centro
            float startX = _graphics.PreferredBackBufferWidth / 2f;
            float startY = _graphics.PreferredBackBufferHeight / 2f;
            _player = new Player(_playerTex, new Vector2(startX, startY), _tileSize, _impassables);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (_state == GameState.Overworld)
            {
                _player.Update(gameTime, _mapData);

                // centraliza a câmera e restringe nos limites
                var halfW = _graphics.PreferredBackBufferWidth / (2f * _camera.Zoom);
                var halfH = _graphics.PreferredBackBufferHeight / (2f * _camera.Zoom);
                var cp = _player.Position;
                cp.X = MathHelper.Clamp(cp.X, halfW, _map.Width - halfW);
                cp.Y = MathHelper.Clamp(cp.Y, halfH, _map.Height - halfH);
                _camera.Position = cp;

                // encontro em relva?
                int tx = (int)(cp.X / _tileSize);
                int ty = (int)(cp.Y / _tileSize);
                if (ty >= 0 && ty < _mapData.GetLength(0)
                 && tx >= 0 && tx < _mapData.GetLength(1)
                 && _mapData[ty, tx] == 2
                 && _rng.NextDouble() < 0.005)
                {
                    _state = GameState.Battle;
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Space))
                _state = GameState.Overworld;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
            _map.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();

            if (_state == GameState.Battle)
            {
                _spriteBatch.Begin();
                _spriteBatch.DrawString(
                    _font,
                    "A wild Pokemon appeared!\n(Press SPACE to continue)",
                    new Vector2(100, 100),
                    Color.White
                );
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
