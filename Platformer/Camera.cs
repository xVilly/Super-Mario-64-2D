using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    public static class Camera
    {
        public static Vector2 WindowSize;
        public static Vector2 mapPosition;
        public static float scale = 1.0f;
        public static Vector2 targetPosition;
        public static Vector2 cameraVelocity;
        public static Vector2 screenCenter;

        public static void Update(float elapsed)
        {
            mapPosition.X = targetPosition.X;
            mapPosition.Y = targetPosition.Y;
            screenCenter = new Vector2(WindowSize.X / 2, WindowSize.Y / 2);
        }
        // Map Positions -> Window position
        public static Vector2 ConvertPos(Vector2 objectPosition)
        {
            return new Vector2(scale * (objectPosition.X - mapPosition.X) + screenCenter.X, scale * (objectPosition.Y - mapPosition.Y) + screenCenter.Y);
        }

        public static Vector2 ConvertPosBack(Vector2 pos)
        {
            return new Vector2((pos.X - screenCenter.X) / scale + mapPosition.X, (pos.Y - screenCenter.Y) / scale + mapPosition.Y);
        }
        public static Vector2 ConvertSize(Vector2 objectSize)
        {
            return objectSize * scale;
        }
        public static Rectangle ConvertRect(Rectangle objectRect)
        {
            return new Rectangle(ConvertPos(new Vector2(objectRect.X, objectRect.Y)).ToPoint(), ConvertSize(new Vector2(objectRect.Width, objectRect.Height)).ToPoint());
        }
    }
}
