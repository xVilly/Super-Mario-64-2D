using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer.GUI
{
    public class ImageWidget : Widget
    {
        private Texture2D texture;
        private int width, height;
        public Color Color;

        public int Top { get { return (int)Position.Y; } }
        public int Right { get { return (int)Position.X + width; } }
        public int Bottom { get { return (int)Position.Y + height; } }
        public int Left { get { return (int)Position.X; } }

        private Rectangle drawRect;

        public ImageWidget(Texture2D _texture = null, int _width = 0, int _height = 0) : base()
        {
            Color = Color.White;
            drawRect = new Rectangle((int)Position.X, (int)Position.Y, 0, 0);
            if (_texture != null)
                SetImage(_texture, _width, _height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null && IsVisible){
                drawRect.X = (int)Position.X;
                drawRect.Y = (int)Position.Y;
                drawRect.Width = width;
                drawRect.Height = height;
                spriteBatch.Draw(texture, drawRect, Color);
            }

            base.Draw(spriteBatch);
        }

        public void SetImage(Texture2D _texture, int _width, int _height)
        {
            texture = _texture;
            width = Math.Max(0, _width);
            height = Math.Max(0, _height);
        }
    }
}
