// UIManager.cs
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian
{
    /// <summary>
    /// Exibe mensagens e menus simples (fugir/lutar) com rato ou teclado (setas/WASD+Enter).
    /// </summary>
    public class UIManager
    {
        private SpriteFont _font;
        public bool IsActive { get; private set; }
        private string[] _options;
        private string _message;
        private int _selected;
        private bool _showOptions;
        private bool _optionChosen;
        private MouseState _prevMouse = Mouse.GetState();

        public int SelectedOption { get; private set; }
        public bool OptionChosen
        {
            get
            {
                if (_optionChosen) { _optionChosen = false; return true; }
                return false;
            }
        }

        public void LoadContent(ContentManager cm)
        {
            _font = cm.Load<SpriteFont>("File");
        }

        public void ShowMessage(string msg)
        {
            _message = msg;
            _options = null;
            _showOptions = false;
            IsActive = true;
        }

        public void ShowOptions(string msg, string[] opts)
        {
            _message = msg;
            _options = opts;
            _selected = 0;
            _showOptions = true;
            _optionChosen = false;
            IsActive = true;
        }

        private List<Rectangle> _optionRects = new List<Rectangle>();

        public void Update(GameTime gt)
        {
            if (!IsActive) return;

            var ks = Keyboard.GetState();
            var ms = Mouse.GetState();

            if (_showOptions)
            {
                // rato
                for (int i = 0; i < _optionRects.Count; i++)
                    if (_optionRects[i].Contains(ms.Position))
                        _selected = i;
                if (ms.LeftButton == ButtonState.Pressed
                 && _prevMouse.LeftButton == ButtonState.Released)
                {
                    SelectedOption = _selected;
                    _optionChosen = true;
                    IsActive = false;
                }

                // teclado setas ou WASD
                if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W))
                    _selected = (_selected - 1 + _options.Length) % _options.Length;
                if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S))
                    _selected = (_selected + 1) % _options.Length;

                if (ks.IsKeyDown(Keys.Enter))
                {
                    SelectedOption = _selected;
                    _optionChosen = true;
                    IsActive = false;
                }
            }
            else
            {
                // mensagem simples
                if (ks.IsKeyDown(Keys.Enter))
                    IsActive = false;
            }

            _prevMouse = ms;
        }

        public void Draw(SpriteBatch sb)
        {
            if (!IsActive) return;

            int W = sb.GraphicsDevice.Viewport.Width;
            int H = sb.GraphicsDevice.Viewport.Height;
            int boxW = W - 20;
            int boxH = _showOptions ? 40 + _options.Length * 20 : 40;
            var rect = new Rectangle(10, H - boxH - 10, boxW, boxH);

            // fundo semitransparente
            sb.Draw(PixelHelper.Pixel, rect, Color.Black * 0.6f);

            // mensagem
            sb.DrawString(_font, _message, new Vector2(rect.X + 10, rect.Y + 10), Color.White);

            _optionRects.Clear();
            if (_showOptions)
            {
                for (int i = 0; i < _options.Length; i++)
                {
                    string txt = (i == _selected ? "--> " : "    ") + _options[i];
                    var pos = new Vector2(rect.X + 20, rect.Y + 30 + i * 20);
                    var col = i == _selected ? Color.Yellow : Color.White;
                    sb.DrawString(_font, txt, pos, col);
                    var meas = _font.MeasureString(txt);
                    _optionRects.Add(new Rectangle(
                        (int)pos.X, (int)pos.Y,
                        (int)meas.X, (int)meas.Y
                    ));
                }
            }
        }
    }
}
