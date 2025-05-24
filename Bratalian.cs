using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Bratalian2
{
    public class Bratalian
    {
        public Vector2 Position;
        public Texture2D Texture;
        public string Name;
        private Vector2 Scale = new Vector2(0.2f, 0.2f);
        public int height;
        public int width;

        public Bratalian(Texture2D texture, Vector2 position, string nome, Vector2 scale)
        {
            Texture = texture;
            Position = position;
            Name = nome;
            Scale = scale;
            height = 24;
            width = 24;

        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        
    }
}
