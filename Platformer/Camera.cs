using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Helpers;

namespace Platformer
{
    public enum CameraMode
    {
        LOCKED,
        MARIO,
        LAKITU
    }
    public enum CameraZoom
    {
        NORMAL,
        IN,
        OUT,
    }
    public enum CameraOffset
    {
        NONE,
        LEFT,
        RIGHT
    }
    public struct CameraSetup
    {
        public CameraMode mode;
        public CameraZoom zoom;
        public CameraOffset offset;
    }
    public static class Camera
    {
        public static Vector2 WindowSize;
        public static Vector2 mapPosition;
        public static float scale = 1.0f;
        public static float scaleVelocity;
        public static Vector2 targetPosition;
        public static Vector2 cameraVelocity;
        public static Vector2 screenCenter;

        public static Entity Actor;
        public static Vector2 FollowPoint;
        public static float targetZoom = 1.0f;

        public static CameraSetup savedCameraSetup; // stores camera setup before lock/cutscene
        private static CameraMode cameraMode;
        public static CameraZoom cameraZoom;
        public static CameraOffset cameraOffset;

        public static void Setup(Entity _actor)
        {
            Actor = _actor;
            cameraMode = CameraMode.LAKITU;
            cameraZoom = CameraZoom.NORMAL;
            cameraOffset = CameraOffset.NONE;
        }

        public static void Update(float elapsed)
        {
            mapPosition.X = targetPosition.X;
            mapPosition.Y = targetPosition.Y;
            screenCenter = new Vector2(WindowSize.X / 2, WindowSize.Y / 2);

            if (cameraMode != CameraMode.LOCKED){
                if (cameraMode == CameraMode.LAKITU){
                    if (cameraOffset != CameraOffset.NONE)
                        FollowPoint = new Vector2(cameraOffset == CameraOffset.LEFT ? Actor.GetHeadPoint().X - 60 : Actor.GetHeadPoint().X + 60, Actor.GetHeadPoint().Y);
                    else
                        FollowPoint = Actor.GetHeadPoint();
                    Camera.targetPosition = Maths.SmoothDamp(Camera.mapPosition, FollowPoint, ref Camera.cameraVelocity, 3.0f, 300.0f, 1.0f);
                } else {
                    if (cameraOffset != CameraOffset.NONE)
                        FollowPoint = new Vector2(cameraOffset == CameraOffset.LEFT ? Actor.GetHeadPoint().X - 30 : Actor.GetHeadPoint().X + 30, Actor.GetHeadPoint().Y);
                    else
                        FollowPoint = Actor.GetHeadPoint();
                    Camera.targetPosition = Maths.SmoothDamp(Camera.mapPosition, FollowPoint, ref Camera.cameraVelocity, 1.5f, 1000.0f, 1.0f);
                }
                scale = Maths.SmoothDamp(scale, targetZoom, ref scaleVelocity, 3.0f, 200.0f, 1.0f);
            }
        }
        // Map Positions -> Window position
        public static Vector2 ConvertPos(Vector2 objectPosition)
        {
            return new Vector2(scale * (objectPosition.X - mapPosition.X) + screenCenter.X, scale * (objectPosition.Y - mapPosition.Y) + screenCenter.Y);
        }

        public static float ConvertXBack(float X)
        {
            return (X - screenCenter.X) / scale + mapPosition.X;
        }
        public static float ConvertYBack(float Y)
        {
            return (Y - screenCenter.Y) / scale + mapPosition.Y;
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


        public static void LockCamera()
        {
            savedCameraSetup = new CameraSetup() { mode = cameraMode, zoom = cameraZoom, offset = cameraOffset };
            cameraMode = CameraMode.LOCKED;
        }

        public static void UnlockCamera()
        {
            if (cameraMode == CameraMode.LOCKED){
                cameraMode = savedCameraSetup.mode;
                cameraZoom = savedCameraSetup.zoom;
                cameraOffset = savedCameraSetup.offset;
            }
        }

        public static CameraMode GetMode()
        {
            return cameraMode;
        }

        public static void ChangeMode(CameraMode _mode)
        {
            cameraMode = _mode;
            if (_mode == CameraMode.MARIO){
                if (cameraZoom == CameraZoom.IN)
                    targetZoom = 2.25f;
                else if (cameraZoom == CameraZoom.NORMAL)
                    targetZoom = 1.75f;
                else if (cameraZoom == CameraZoom.OUT)
                    targetZoom = 1.5f;
            } else if (_mode == CameraMode.LAKITU) {
                if (cameraZoom == CameraZoom.IN)
                    targetZoom = 1.75f;
                else if (cameraZoom == CameraZoom.NORMAL)
                    targetZoom = 1.25f;
                else if (cameraZoom == CameraZoom.OUT)
                    targetZoom = 0.90f;
            }
        }

        public static void ChangeZoom(CameraZoom _zoom){
            cameraZoom = _zoom;
            if (cameraMode == CameraMode.MARIO){
                if (cameraZoom == CameraZoom.IN)
                    targetZoom = 2.25f;
                else if (cameraZoom == CameraZoom.NORMAL)
                    targetZoom = 1.75f;
                else if (cameraZoom == CameraZoom.OUT)
                    targetZoom = 1.5f;
            }
            else if (cameraMode == CameraMode.LAKITU){
                if (cameraZoom == CameraZoom.IN)
                    targetZoom = 1.75f;
                else if (cameraZoom == CameraZoom.NORMAL)
                    targetZoom = 1.25f;
                else if (cameraZoom == CameraZoom.OUT)
                    targetZoom = 0.90f;
            }
        }

        public static void ChangeOffset(CameraOffset _offset)
        {
            cameraOffset = _offset;
        }
    }
}
