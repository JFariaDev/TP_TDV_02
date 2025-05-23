// Game1.cs
using System;
using System.Linq;
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
        private UIManager _ui;
        private PartyUI _partyUI;

        // guardamos os dados do party
        private BratalianData[] _partyData;

        private Point _lastTile = new Point(int.MinValue, int.MinValue);
        private Random _rnd = new Random();
        private const double EncounterChance = 0.05;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // cria pixel 1x1 branco
            PixelHelper.Pixel = new Texture2D(GraphicsDevice, 1, 1);
            PixelHelper.Pixel.SetData(new[] { Color.White });

            // inicializa mapa
            _map = new Map(seed: 12345);
            _map.LoadContent(Content);

            // inicializa player
            _player = new Player(new Vector2(0, 0), speed: 120f);
            _player.LoadContent(Content);

            // inicializa UI de mensagem/menu
            _ui = new UIManager();
            _ui.LoadContent(Content);

            // carrega a base de dados
            BratalianDB.Load();

            // escolhe ids 1..6 para a party
            var partyIds = new[] { 1, 2, 3, 4, 5, 6 };

            // carrega os dados do Bratalian e guarda no array
            _partyData = partyIds
                .Select(id => BratalianDB.Get(id))
                .Where(d => d != null)
                .ToArray();

            // carrega os sprites a partir do campo spriteAsset
            var sprites = _partyData
                .Select(d => Content.Load<Texture2D>(d.spriteAsset))
                .ToArray();

            // inicializa UI de selecao de party
            _partyUI = new PartyUI(sprites);
            _partyUI.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            // 1) menu fugir/lutar
            if (_ui.IsActive)
            {
                _ui.Update(gameTime);
                if (_ui.OptionChosen)
                {
                    if (_ui.SelectedOption == 0) // Fugir
                        _ui.ShowMessage("Fugiste do combate!");
                    else                        // Lutar
                        _partyUI.Show();
                }
                return;
            }

            // 2) selecao de Bratalian
            if (_partyUI.IsActive)
            {
                _partyUI.Update(gameTime);
                if (_partyUI.SelectionMade)
                {
                    int idx = _partyUI.SelectedIndex;
                    // usa o nome da DB em vez do numero
                    var nome = _partyData[idx].name;
                    _ui.ShowMessage($"Selecionaste o {nome}!");
                }
                return;
            }

            // 3) logica normal
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _player.Update(gameTime);

            int tx = (int)Math.Floor(_player.Position.X / Map.TileSize);
            int ty = (int)Math.Floor(_player.Position.Y / Map.TileSize);
            var cur = new Point(tx, ty);

            if (cur != _lastTile)
            {
                if (_map.IsGrassTile(tx, ty) && _rnd.NextDouble() < EncounterChance)
                    _ui.ShowOptions("Encontraste um Bratalian!", new[] { "Fugir", "Lutar" });
                _lastTile = cur;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // câmara segue o player
            Viewport vp = GraphicsDevice.Viewport;
            Vector2 center = new Vector2(vp.Width, vp.Height) * 0.5f;

            var view = Matrix.CreateTranslation(
                          -_player.Position.X,
                          -_player.Position.Y,
                          0f
                       ) * Matrix.CreateTranslation(center.X, center.Y, 0f);

            // calcula rect visivel em coordenadas de mundo
            var inv = Matrix.Invert(view);
            var topL = Vector2.Transform(Vector2.Zero, inv);
            var botR = Vector2.Transform(new Vector2(vp.Width, vp.Height), inv);
            var viewRect = new Rectangle(
                (int)topL.X, (int)topL.Y,
                (int)(botR.X - topL.X), (int)(botR.Y - topL.Y)
            );

            // desenha mapa e player
            _spriteBatch.Begin(transformMatrix: view);
            _map.Draw(_spriteBatch, viewRect);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();

            // desenha UIs por cima
            _spriteBatch.Begin();
            _ui.Draw(_spriteBatch);
            _partyUI.Draw(_spriteBatch);
            _spriteBatch.End();
        }
    }
}
