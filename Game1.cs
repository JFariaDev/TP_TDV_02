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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // cria e carrega o mapa
            _map = new Map(seed: 12345);
            _map.LoadContent(Content);

            // cria e carrega o player
            _player = new Player(new Vector2(0, 0), speed: 120f);
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

            // câmara que segue o player
            Viewport vp = GraphicsDevice.Viewport;
            Vector2 center = new Vector2(vp.Width, vp.Height) * 0.5f;

            var view =
                Matrix.CreateTranslation(new Vector3(-_player.Position, 0f)) *
                Matrix.CreateScale(1f, 1f, 1f) *
                Matrix.CreateTranslation(new Vector3(center, 0f));

            // calcula viewRect em coords de mundo
            var inv = Matrix.Invert(view);
            var topL = Vector2.Transform(Vector2.Zero, inv);
            var botR = Vector2.Transform(new Vector2(vp.Width, vp.Height), inv);
            var viewRect = new Rectangle(
                (int)topL.X, (int)topL.Y,
                (int)(botR.X - topL.X),
                (int)(botR.Y - topL.Y)
            );

            _spriteBatch.Begin(transformMatrix: view);
            _map.Draw(_spriteBatch, viewRect);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
