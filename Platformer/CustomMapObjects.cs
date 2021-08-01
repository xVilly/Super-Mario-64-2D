using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Helpers;
using System.Diagnostics;

namespace Platformer
{
    public enum SolidObject
    {
        Rectangle = 0,
        Slope = 1
    }
    public class RectObject : MapObject
    {
        public RectObject(Vector2 startPosition, Vector2 size) : base(startPosition)
        {
            this.size = size;
            this.RectObject = this;
            Game1.mapRectangles.Add(this);
            type = SolidObject.Rectangle;
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            

            if (texture != null)
                spriteBatch.Draw(texture, Camera.ConvertRect(new Rectangle(position.ToPoint(), size.ToPoint())), texture.Bounds, Color.White, 0.0f, new Vector2(0,0), SpriteEffects.None, 1.0f);
            if (Game1.MAP_HITBOX)
            {
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X, position.Y)), Camera.ConvertPos(new Vector2(position.X + size.X, position.Y)), Color.Black, 1);
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X, position.Y)), Camera.ConvertPos(new Vector2(position.X, position.Y + size.Y)), Color.Black, 1);
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X, position.Y + size.Y)), Camera.ConvertPos(new Vector2(position.X + size.X, position.Y + size.Y)), Color.Black, 1);
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X + size.X, position.Y)), Camera.ConvertPos(new Vector2(position.X + size.X, position.Y + size.Y)), Color.Black, 1);
            }
        }
    }
    public class SlopeObject : MapObject
    {
        public bool direction; // false-> w prawo, true-> w lewo
        public double incline
        {
            get
            {
                return size.Y / size.X;
            }
        }
        
        public SlopeObject(Vector2 startPosition, Vector2 size, bool direction = false) : base(startPosition)
        {
            this.size = size;
            this.direction = direction;
            this.SlopeObject = this;
            Game1.mapSlopes.Add(this);
            type = SolidObject.Slope;
        }



        public Vector2[] GetTrianglePoints()
        {
            Vector2 _position = Camera.ConvertPos(position);
            Vector2 _size = Camera.ConvertSize(size);
            Vector2[] points = new Vector2[3];
            if (!direction)
            {
                points[0] = _position;
                points[1] = new Vector2(_position.X, _position.Y + _size.Y);
                points[2] = new Vector2(_position.X + _size.X, _position.Y + _size.Y);
            }
            else
            {
                points[0] = new Vector2(_position.X + _size.X, _position.Y);
                points[1] = new Vector2(_position.X + _size.X, _position.Y + _size.Y);
                points[2] = new Vector2(_position.X, _position.Y + _size.Y);

            }
            return points;
        }

        public Vector2[] GetTrianglePointsRaw()
        {
            Vector2 _position = position;
            Vector2 _size = size;
            Vector2[] points = new Vector2[3];
            if (!direction)
            {
                points[0] = _position;
                points[1] = new Vector2(_position.X, _position.Y + _size.Y);
                points[2] = new Vector2(_position.X + _size.X, _position.Y + _size.Y);
            }
            else
            {
                points[0] = new Vector2(_position.X + _size.X, _position.Y);
                points[1] = new Vector2(_position.X + _size.X, _position.Y + _size.Y);
                points[2] = new Vector2(_position.X, _position.Y + _size.Y);

            }
            return points;
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            // * HITBOX *
            

            // TEXTURE
            if (texture != null)
            {
                EffectTechnique effectTechnique = basicEffect.CurrentTechnique;
                EffectPassCollection effectPassCollection = effectTechnique.Passes;
                basicEffect.TextureEnabled = true;
                basicEffect.Texture = texture;

                var vertices = new VertexPositionTexture[]
                {
                    new VertexPositionTexture(new Vector3(GetTrianglePoints()[0].X, GetTrianglePoints()[0].Y, 0), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(GetTrianglePoints()[1].X, GetTrianglePoints()[1].Y, 0), new Vector2(0, 1)),
                    new VertexPositionTexture(new Vector3(GetTrianglePoints()[2].X, GetTrianglePoints()[2].Y, 0), new Vector2(1, 1)),
                };
                var buffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
                buffer.SetData(vertices);
                graphicsDevice.SetVertexBuffer(buffer);
                graphicsDevice.RasterizerState = RasterizerState.CullNone;

                foreach (EffectPass pass in effectPassCollection)
                {
                    pass.Apply();

                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
                        vertices, 0, 1);
                }
            }

            if (Game1.MAP_HITBOX)
            {
                Maths.DrawLine(spriteBatch, GetTrianglePoints()[0], GetTrianglePoints()[1], Color.Black, 1);
                Maths.DrawLine(spriteBatch, GetTrianglePoints()[1], GetTrianglePoints()[2], Color.Black, 1);
                Maths.DrawLine(spriteBatch, GetTrianglePoints()[2], GetTrianglePoints()[0], Color.Black, 1);
            }
        }
    }
}
