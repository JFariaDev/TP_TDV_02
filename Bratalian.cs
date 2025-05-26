using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Bratalian2
{
    public class Bratalian
    {
        public Vector2 Position;
        public Texture2D Texture;
        public string Name;
        public Vector2 Scale = new Vector2(0.2f, 0.2f);
        public int Height => Texture?.Height ?? 0;
        public int Width => Texture?.Width ?? 0;

        public string Type1;
        public string? Type2; 
        public List<Attack> Attacks = new List<Attack>();
        public bool IsDefeated { get; set; } = false;


        public Bratalian(Texture2D texture, Vector2 position, string nome, Vector2 scale, string type1, string? type2 = null)
        {
            Texture = texture;
            Position = position;
            Name = nome;
            Scale = scale;
            Type1 = type1;
            Type2 = type2;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * Scale.X), (int)(Height * Scale.Y));
        }
    }
}
