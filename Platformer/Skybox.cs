using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    public class Skybox
    {
        private Texture2D texture;
        private Color ambientColor;

        public Color AmbientColor { get { return ambientColor; } }

        public Skybox(Color _ambientColor, int _textureId = -1)
        {
            ambientColor = _ambientColor;
            texture = SpriteManager.GetSkyboxTexture(_textureId);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture == null){
                spriteBatch.Draw(GameManager.defaultTexture, new Rectangle(0, 0, (int)Camera.WindowSize.X, (int)Camera.WindowSize.Y), texture.Bounds, ambientColor, 1.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            } else {
                spriteBatch.Draw(texture, new Rectangle(0, 0, (int)Camera.WindowSize.X, (int)Camera.WindowSize.Y), texture.Bounds, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            }
        }
    }
}
