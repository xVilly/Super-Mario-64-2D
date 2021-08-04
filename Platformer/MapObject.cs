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

    public abstract class MapObject
    {
        protected SlopeObject SlopeObject = null;
        protected RectObject RectObject = null;

        public Vector2 position;
        public Texture2D texture;
        public Vector2 size;
        public SolidObject type;
        public bool collision;
        public float Friction = 3.0f;

        public MapObject(Vector2 startPosition)
        {
            collision = true;
            position = startPosition;
            Game1.mapObjects.Add(this);
        }

        public MapObject GetMapObject() { return this; }
        public SlopeObject GetSlopeObject() { return SlopeObject; }
        public RectObject GetRectObject() { return RectObject; }

        public abstract void Initialize(GraphicsDevice graphicsDevice);
        public abstract Vector2[] GetVertices(bool cameraView = false);
        public abstract void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, BasicEffect basicEffect);

        // MapObject static methods
        public static MapObject GetObjectFromPos(Vector2 pos, bool collider = false)
        {
            foreach (MapObject obj in Game1.mapObjects)
            {
                if (obj.type == SolidObject.Slope)
                {
                    SlopeObject slope = obj.GetSlopeObject();
                    if (Maths.PointInTriangle(pos, slope.GetVertices()[0], slope.GetVertices()[1], slope.GetVertices()[2]) && (!collider || (collider && obj.collision)))
                        return obj;
                }
                else
                {
                    if (Maths.PointInRectangle(pos, new Rectangle(obj.position.ToPoint(), obj.size.ToPoint())) && (!collider || (collider && obj.collision)))
                        return obj;
                }
            }
            return null;
        }
        public bool IsInView()
        {
            float LeftScreenBounds = Camera.ConvertXBack(Camera.screenCenter.X - (Camera.WindowSize.X / 2));
            if (position.X + size.X < LeftScreenBounds)
                return false;
            float RightScreenBounds = Camera.ConvertXBack(Camera.screenCenter.X + (Camera.WindowSize.X / 2));
            if (position.X > RightScreenBounds)
                return false;
            float TopScreenBounds = Camera.ConvertYBack(Camera.screenCenter.Y - (Camera.WindowSize.Y / 2));
            if (position.Y + size.Y < TopScreenBounds)
                return false;
            float BottomScreenBounds = Camera.ConvertYBack(Camera.screenCenter.Y + (Camera.WindowSize.Y / 2));
            if (position.Y > BottomScreenBounds)
                return false;
            return true;
        }
    }

    public class RectObject : MapObject
    {
        private VertexPositionTexture[] vertexData;
        private VertexBuffer vertexBuffer;

        public RectObject(Vector2 startPosition, Vector2 size) : base(startPosition)
        {
            this.size = size;
            this.RectObject = this;
            Game1.mapRectangles.Add(this);
            type = SolidObject.Rectangle;
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            vertexData = new VertexPositionTexture[]{
                 new VertexPositionTexture(Vector3.Zero, new Vector2(0, 0)),
                 new VertexPositionTexture(Vector3.Zero, new Vector2(1, 0)),
                 new VertexPositionTexture(Vector3.Zero, new Vector2(0, 1)),
                 new VertexPositionTexture(Vector3.Zero, new Vector2(1, 0)),
                 new VertexPositionTexture(Vector3.Zero, new Vector2(0, 1)),
                 new VertexPositionTexture(Vector3.Zero, new Vector2(1, 1)),
            };
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), vertexData.Length, BufferUsage.WriteOnly);
        }

        public override Vector2[] GetVertices(bool cameraView = false)
        {
            Vector2[] vertices = new Vector2[4];
            Vector2 _pos = position;
            Vector2 _size = size;
            if (cameraView){
                _pos = Camera.ConvertPos(position);
                _size = Camera.ConvertSize(size);
            }
            vertices[0] = _pos;
            vertices[1] = new Vector2(_pos.X + _size.X, _pos.Y);
            vertices[2] = new Vector2(_pos.X, _pos.Y + _size.Y);
            vertices[3] = new Vector2(_pos.X + _size.X, _pos.Y + _size.Y);
            return vertices;
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            vertexData[0].Position = new Vector3(GetVertices(true)[0], 0);
            vertexData[1].Position = new Vector3(GetVertices(true)[1], 0);
            vertexData[2].Position = new Vector3(GetVertices(true)[2], 0);
            vertexData[3].Position = vertexData[1].Position;
            vertexData[4].Position = vertexData[2].Position;
            vertexData[5].Position = new Vector3(GetVertices(true)[3], 0);
            if (texture != null){
                vertexBuffer.SetData(vertexData);
                graphicsDevice.SetVertexBuffer(vertexBuffer);
                basicEffect.Texture = texture;
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes){
                    pass.Apply();
                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                }
            }

            // debug
            if (Game1.MAP_HITBOX) {
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X, position.Y)), Camera.ConvertPos(new Vector2(position.X + size.X, position.Y)), Color.Black, 1);
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X, position.Y)), Camera.ConvertPos(new Vector2(position.X, position.Y + size.Y)), Color.Black, 1);
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X, position.Y + size.Y)), Camera.ConvertPos(new Vector2(position.X + size.X, position.Y + size.Y)), Color.Black, 1);
                Maths.DrawLine(spriteBatch, Camera.ConvertPos(new Vector2(position.X + size.X, position.Y)), Camera.ConvertPos(new Vector2(position.X + size.X, position.Y + size.Y)), Color.Black, 1);
            }
        }
    }

    public class SlopeObject : MapObject
    {
        private VertexPositionTexture[] vertexData;
        private VertexBuffer vertexBuffer;

        public bool direction;
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

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            vertexData = new VertexPositionTexture[]{
                 new VertexPositionTexture(Vector3.Zero, new Vector2(0, 0)),
                 new VertexPositionTexture(Vector3.Zero, new Vector2(1, 0)),
                 new VertexPositionTexture(Vector3.Zero, new Vector2(0, 1)),
            };
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), vertexData.Length, BufferUsage.WriteOnly);
        }

        public override Vector2[] GetVertices(bool cameraView = false)
        {
            Vector2[] vertices = new Vector2[3];
            Vector2 _pos = position;
            Vector2 _size = size;
            if (cameraView){
                _pos = Camera.ConvertPos(position);
                _size = Camera.ConvertSize(size);
            }
            if (!direction) {
                vertices[0] = _pos;
                vertices[1] = new Vector2(_pos.X, _pos.Y + _size.Y);
                vertices[2] = new Vector2(_pos.X + _size.X, _pos.Y + _size.Y);
            }else{
                vertices[0] = new Vector2(_pos.X + _size.X, _pos.Y);
                vertices[1] = new Vector2(_pos.X + _size.X, _pos.Y + _size.Y);
                vertices[2] = new Vector2(_pos.X, _pos.Y + _size.Y);
            }
            return vertices;
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            vertexData[0].Position = new Vector3(GetVertices(true)[0], 0);
            vertexData[1].Position = new Vector3(GetVertices(true)[1], 0);
            vertexData[2].Position = new Vector3(GetVertices(true)[2], 0);
            if (texture != null){
                basicEffect.Texture = texture;
                vertexBuffer.SetData(vertexData);
                graphicsDevice.SetVertexBuffer(vertexBuffer);
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
                    pass.Apply();
                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                }
            }
            // debug
            if (Game1.MAP_HITBOX){
                Maths.DrawLine(spriteBatch, GetVertices(true)[0], GetVertices(true)[1], Color.Black, 1);
                Maths.DrawLine(spriteBatch, GetVertices(true)[1], GetVertices(true)[2], Color.Black, 1);
                Maths.DrawLine(spriteBatch, GetVertices(true)[2], GetVertices(true)[0], Color.Black, 1);
            }
        }
    }
}
