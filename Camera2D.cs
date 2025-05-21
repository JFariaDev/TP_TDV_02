using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian
{
    public class Camera2D
    {
        private readonly Viewport _viewport;
        public Vector2 Position;
        public float Zoom = 2f;

        public Camera2D(Viewport viewport)
        {
            _viewport = viewport;
        }

        public Matrix GetViewMatrix()
        {
            return
                Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
                Matrix.CreateScale(Zoom, Zoom, 1f) *
                Matrix.CreateTranslation(new Vector3(_viewport.Width / 2f, _viewport.Height / 2f, 0f));
        }
    }
}
