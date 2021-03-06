using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Platformer.Helpers
{
    
    public static class Maths
    {
        internal static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        internal static bool sameSign(float num1, float num2)
        {
            return num1 >= 0 && num2 >= 0 || num1 < 0 && num2 < 0;
        }

        internal static float GetSlopeProjectedY(SlopeObject slopeObject, float x)
        {
            if (slopeObject.direction)
            {
                float X = slopeObject.position.X;
                float Y = slopeObject.position.Y;
                float W = slopeObject.size.X;
                float H = slopeObject.size.Y;
                float projectedY = Y + H - (((x - X) / ((X + W) - X) * (H - 0)) + 0);
                return projectedY;
            }
            else
            {
                float X = slopeObject.position.X;
                float Y = slopeObject.position.Y;
                float W = slopeObject.size.X;
                float H = slopeObject.size.Y;
                float projectedY = Y + H - (((x - X) / ((X + W) - X) * (0 - H)) + H);
                return projectedY;
            }
        }

        internal static long GetMs()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        internal static bool InWindow(long windowStart, int ms)
        {
            return GetMs() >= windowStart && GetMs() <= windowStart + ms;
        }

        internal static bool AfterWindow(long windowStart, int ms)
        {
            return GetMs() > windowStart + ms;
        }

        internal static Vector2 GetClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
        {
            Vector2 AP = P - A;       //Vector from A to P   
            Vector2 AB = B - A;       //Vector from A to B  

            float magnitudeAB = AB.LengthSquared();     //Magnitude of AB vector (it's length squared)     
            float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;

            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }

        internal static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            
            return !(has_neg && has_pos);
        }

        internal static bool PointInRectangle(Vector2 pt, Rectangle rect)
        {
            return rect.Contains(pt.ToPoint());
        }

        internal static bool Overlap(Rectangle RectA, Rectangle RectB)
        {
            return (RectA.Left < RectB.Right && RectA.Right > RectB.Left &&
     RectA.Top > RectB.Bottom && RectA.Bottom < RectB.Top);
        }

        internal static bool Overlap(Circle cir, Rectangle rect)
        {
            float rW = (rect.Width) / 2;
            float rH = (rect.Height) / 2;
            float distX = Math.Abs(cir.Center.X - (rect.Left + rW));
            float distY = Math.Abs(cir.Center.Y - (rect.Top + rH));

            if (distX >= cir.Radius + rW || distY >= cir.Radius + rH)
                return false;
            if (distX < rW || distY < rH)
                return true;
            distX -= rW;
            distY -= rH;
            if (distX * distX + distY * distY < cir.Radius * cir.Radius)
                return true;
            return false;
        }

        // Fast functions (no need for creating new rectangles/vectors/circles)
        internal static bool FOverlap(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
        {
            return (x1 < x2 + w2 && x1 + w1 > x2 && y1 > y2 + h2 && y1 + h1 < y2);
        }

        internal static bool FOverlap(float cx, float cy, float cr, float x, float y, float w, float h)
        {
            float rW = (w) / 2;
            float rH = (h) / 2;
            float distX = Math.Abs(cx - (x + rW));
            float distY = Math.Abs(cy - (y + rH));

            if (distX >= cr + rW || distY >= cr + rH)
                return false;
            if (distX < rW || distY < rH)
                return true;
            distX -= rW;
            distY -= rH;
            if (distX * distX + distY * distY < cr * cr)
                return true;
            return false;
        }

        internal static bool FOverlap(float x1, float y1, float r1, float x2, float y2, float r2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) < r1 + r2;
        }

        internal static bool FPointInRectangle(float x, float y, float rx, float ry, float rw, float rh)
        {
            return x >= rx && x < rx + rw && y >= ry && y < ry + rh;
        }
        // ============================================================================

        internal static bool ArraySplitterFound(byte[] arr, byte[] splitter, int currentIndex)
        {
            int occurences = 0;
            for (int i = 1; i <= splitter.Length; i++)
            {
                if (arr[currentIndex - i] == splitter[splitter.Length - i])
                {
                    occurences++;
                }
            }
            if (occurences >= splitter.Length)
                return true;
            return false;
        }

        private static Texture2D _texture;
        private static Texture2D GetTexture(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _texture.SetData(new[] { Color.White });
            }

            return _texture;
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Math.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;
            float originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = Math.Clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            // Prevent overshooting
            if (originalTo - current > 0.0F == output > originalTo)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

        public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Math.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);

            float change_x = current.X - target.X;
            float change_y = current.Y - target.Y;
            Vector2 originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;

            float maxChangeSq = maxChange * maxChange;
            float sqDist = change_x * change_x + change_y * change_y;
            if (sqDist > maxChangeSq)
            {
                var mag = (float)Math.Sqrt(sqDist);
                change_x = change_x / mag * maxChange;
                change_y = change_y / mag * maxChange;
            }

            target.X = current.X - change_x;
            target.Y = current.Y - change_y;

            float temp_x = (currentVelocity.X + omega * change_x) * deltaTime;
            float temp_y = (currentVelocity.Y + omega * change_y) * deltaTime;

            currentVelocity.X = (currentVelocity.X - omega * temp_x) * exp;
            currentVelocity.Y = (currentVelocity.Y - omega * temp_y) * exp;

            float output_x = target.X + (change_x + temp_x) * exp;
            float output_y = target.Y + (change_y + temp_y) * exp;

            // Prevent overshooting
            float origMinusCurrent_x = originalTo.X - current.X;
            float origMinusCurrent_y = originalTo.Y - current.Y;
            float outMinusOrig_x = output_x - originalTo.X;
            float outMinusOrig_y = output_y - originalTo.Y;

            if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y > 0)
            {
                output_x = originalTo.X;
                output_y = originalTo.Y;

                currentVelocity.X = (output_x - originalTo.X) / deltaTime;
                currentVelocity.Y = (output_y - originalTo.Y) / deltaTime;
            }
            return new Vector2(output_x, output_y);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

    }
}
