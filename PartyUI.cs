// PartyUI.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    /// <summary>
    /// UI para escolha de 6 Bratalian (2x3), navegavel por rato ou teclado (WASD/arrow+Enter).
    /// </summary>
    public class PartyUI
    {
        private Texture2D[] _sprites;
        private Rectangle[] _slots = new Rectangle[6];
        private int _selected;
        private bool _isActive;
        private bool _selectionMade;
        private MouseState _prevMouse;
        private SpriteFont _font;

        public bool IsActive => _isActive;
        public bool SelectionMade => _selectionMade;
        public int SelectedIndex { get; private set; }

        public PartyUI(Texture2D[] sprites)
        {
            if (sprites == null || sprites.Length != 6)
                throw new ArgumentException("Sao precisos 6 sprites");
            _sprites = sprites;
        }

        public void LoadContent(ContentManager cm)
        {
            _font = cm.Load<SpriteFont>("File");
        }

        public void Show()
        {
            _isActive = true;
            _selectionMade = false;
            _selected = 0;
            _prevMouse = Mouse.GetState();
        }

        public void Update(GameTime gt)
        {
            if (!_isActive) return;

            var ks = Keyboard.GetState();
            var ms = Mouse.GetState();

            // rato
            for (int i = 0; i < 6; i++)
                if (_slots[i].Contains(ms.Position))
                    _selected = i;
            if (ms.LeftButton == ButtonState.Pressed
             && _prevMouse.LeftButton == ButtonState.Released)
            {
                SelectedIndex = _selected;
                _selectionMade = true;
                _isActive = false;
            }

            // teclado WASD ou arrows
            int row = _selected / 3, col = _selected % 3;
            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A))
                col = (col + 2) % 3;
            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D))
                col = (col + 1) % 3;
            if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W))
                row = (row + 1) % 2;
            if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S))
                row = (row + 1) % 2;

            int newSel = row * 3 + col;
            if (newSel != _selected)
                _selected = newSel;

            if (ks.IsKeyDown(Keys.Enter))
            {
                SelectedIndex = _selected;
                _selectionMade = true;
                _isActive = false;
            }

            _prevMouse = ms;
        }

        public void Draw(SpriteBatch sb)
        {
            if (!_isActive) return;

            int W = sb.GraphicsDevice.Viewport.Width;
            int H = sb.GraphicsDevice.Viewport.Height;

            // overlay semitransparente
            sb.Draw(PixelHelper.Pixel, new Rectangle(0, 0, W, H), Color.Black * 0.7f);

            // titulo centralizado
            const string title = "SELECIONA UM BRATALIAN";
            var msize = _font.MeasureString(title);
            sb.DrawString(_font, title, new Vector2((W - msize.X) / 2, 50), Color.White);

            // grid 2 linhas x 3 colunas
            int icon = 64, margin = 20;
            int gridW = icon * 3 + margin * 2;
            int gridH = icon * 2 + margin;
            int ox = (W - gridW) / 2;
            int oy = (H - gridH) / 2;

            for (int i = 0; i < 6; i++)
            {
                int r = i / 3, c = i % 3;
                int x = ox + c * (icon + margin);
                int y = oy + r * (icon + margin);
                var dst = new Rectangle(x, y, icon, icon);
                _slots[i] = dst;
                sb.Draw(_sprites[i], dst, Color.White);

                if (i == _selected)
                {
                    int t = 4;
                    // borda amarela
                    sb.Draw(PixelHelper.Pixel, new Rectangle(x, y, icon, t), Color.Yellow);
                    sb.Draw(PixelHelper.Pixel, new Rectangle(x, y, t, icon), Color.Yellow);
                    sb.Draw(PixelHelper.Pixel, new Rectangle(x, y + icon - t, icon, t), Color.Yellow);
                    sb.Draw(PixelHelper.Pixel, new Rectangle(x + icon - t, y, t, icon), Color.Yellow);
                }
            }
        }
    }
}
