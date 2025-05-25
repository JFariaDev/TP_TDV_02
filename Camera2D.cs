using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bratalian2
{
    public class Camera2D
    {
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Quanto a câmera está “zoominada”. Ajusta para 1.3f ou 1.5f
        /// </summary>
        public float Zoom { get; set; } = 2f;

        /// <summary>
        /// Rotação da câmera (normalmente 0)
        /// </summary>
        public float Rotation { get; set; } = 0f;

        private readonly Viewport _viewport;

        public Camera2D(Viewport viewport)
        {
            _viewport = viewport;
        }

        /// <summary>
        /// Retorna a matriz de visão que mantém 
        /// sempre o target (player) centrado na tela.
        /// </summary>
        public Matrix GetViewMatrix(Vector2 targetPosition)
        {
            // Centro da tela em pixels
            var screenCenter = new Vector2(
                _viewport.Width / 2f,
                _viewport.Height / 2f);

            Position = targetPosition;

            return
                // 1) Muda origem para o ponto que queremos centrar
                Matrix.CreateTranslation(-Position.X, -Position.Y, 0f)
             // 2) Aplica rotação (se houver)
             * Matrix.CreateRotationZ(Rotation)
             // 3) Aplica zoom
             * Matrix.CreateScale(Zoom)
             // 4) Translada para o centro real da viewport
             * Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0f);
        }
    }
}
