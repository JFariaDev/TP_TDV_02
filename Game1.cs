// Game1.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _batch;
        Map _map;
        Player _player;
        const float Zoom = 1f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _batch = new SpriteBatch(GraphicsDevice);

            _map = new Map(seed: 12345);
            _map.LoadContent(Content);

            _player = new Player(new Vector2(0, 0), 120f);
            _player.LoadContent(Content);
        }

        protected override void Update(GameTime gt)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            _player.Update(gt);
            base.Update(gt);
        }

        protected override void Draw(GameTime gt)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Viewport vp = GraphicsDevice.Viewport;
            Vector2 cen = new Vector2(vp.Width, vp.Height) * 0.5f;
            var view =
                Matrix.CreateTranslation(-_player.Position.X, -_player.Position.Y, 0) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(cen.X, cen.Y, 0);

            // calcula viewRect em px de mundo
            var inv = Matrix.Invert(view);
            var tl = Vector2.Transform(Vector2.Zero, inv);
            var br = Vector2.Transform(new Vector2(vp.Width, vp.Height), inv);
            Rectangle vr = new((int)tl.X, (int)tl.Y,
                               (int)(br.X - tl.X), (int)(br.Y - tl.Y));

            _batch.Begin(transformMatrix: view);
            _map.Draw(_batch, vr);
            _player.Draw(_batch);
            _batch.End();

            base.Draw(gt);
        }
    }
}
