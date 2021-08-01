using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Helpers;

namespace Platformer
{
    public abstract class MapObject
    {
        // Map Objects Constants

        protected SlopeObject SlopeObject = null;
        protected RectObject RectObject = null;


        public Vector2 position;
        public Texture2D texture;
        public Vector2 size;

        public SolidObject type;
        public bool collision = true;


        public float Friction = 3.0f;


        public MapObject(Vector2 startPosition)
        {
            position = startPosition;
            Game1.mapObjects.Add(this);
        }

        public MapObject GetMapObject() { return this; }
        public SlopeObject GetSlopeObject() { return SlopeObject; }
        public RectObject GetRectObject() { return RectObject; }


        public abstract void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, BasicEffect basicEffect);

        public static MapObject GetObjectFromPos(Vector2 pos, bool collider = false)
        {
            foreach (MapObject obj in Game1.mapObjects)
            {
                if (obj.type == SolidObject.Slope)
                {
                    SlopeObject slope = obj.GetSlopeObject();
                    if (Maths.PointInTriangle(pos, slope.GetTrianglePointsRaw()[0], slope.GetTrianglePointsRaw()[1], slope.GetTrianglePointsRaw()[2]) && (!collider || (collider && obj.collision)))
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
    }
}
