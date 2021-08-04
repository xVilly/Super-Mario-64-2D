using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    public struct InputButton
    {
        public KeyState state;
        public KeyState prevState;
        public bool IsPressed()
        {
            return state == KeyState.Down;
        }
        public bool IsReleased()
        {
            return state == KeyState.Up;
        }
        public bool WasPressed()
        {
            return prevState == KeyState.Down;
        }
        public bool WasReleased()
        {
            return prevState == KeyState.Up;
        }
        public bool OnPress()
        {
            return state == KeyState.Down && prevState == KeyState.Up;
        }
        public bool OnRelease()
        {
            return state == KeyState.Up && prevState == KeyState.Down;
        }
    }
    public static class InputManager
    {
        public static bool UseController = false;

        public static bool LockGameInput = false;
        public static float HorizontalInput = 0.0f;
        public static InputButton JumpButton = new InputButton() {state = KeyState.Up, prevState = KeyState.Up };
        public static InputButton CrouchButton = new InputButton() { state = KeyState.Up, prevState = KeyState.Up };
        public static InputButton AttackButton = new InputButton() { state = KeyState.Up, prevState = KeyState.Up };

        public static InputButton CameraMode = new InputButton() { state = KeyState.Up, prevState = KeyState.Up };
        public static InputButton CameraLeft = new InputButton() { state = KeyState.Up, prevState = KeyState.Up };
        public static InputButton CameraRight = new InputButton() { state = KeyState.Up, prevState = KeyState.Up };
        public static InputButton CameraUp = new InputButton() { state = KeyState.Up, prevState = KeyState.Up };
        public static InputButton CameraDown = new InputButton() { state = KeyState.Up, prevState = KeyState.Up };
        public static void Update()
        {
            JumpButton.prevState = JumpButton.state;
            CrouchButton.prevState = CrouchButton.state;
            AttackButton.prevState = AttackButton.state;
            CameraMode.prevState = CameraMode.state;
            CameraLeft.prevState = CameraLeft.state;
            CameraRight.prevState = CameraRight.state;
            CameraUp.prevState = CameraUp.state;
            CameraDown.prevState = CameraDown.state;

            if (LockGameInput)
                return;
            if (UseController)
            {
                HorizontalInput = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X;
                JumpButton.state = GamePad.GetState(0).Buttons.A == ButtonState.Released ? KeyState.Up : KeyState.Down;
                CrouchButton.state = GamePad.GetState(0).Buttons.LeftShoulder == ButtonState.Released ? KeyState.Up : KeyState.Down;
                AttackButton.state = GamePad.GetState(0).Buttons.X == ButtonState.Released ? KeyState.Up : KeyState.Down;
                CameraMode.state = GamePad.GetState(0).Buttons.B == ButtonState.Released ? KeyState.Up : KeyState.Down;
                CameraLeft.state = GamePad.GetState(0).Buttons.LeftShoulder == ButtonState.Released ? KeyState.Up : KeyState.Down;
                CameraRight.state = GamePad.GetState(0).Buttons.RightShoulder == ButtonState.Released ? KeyState.Up : KeyState.Down;
                CameraUp.state = GamePad.GetState(0).ThumbSticks.Right.Y < -0.5 ? KeyState.Up : KeyState.Down;
                CameraDown.state = GamePad.GetState(0).ThumbSticks.Right.Y > 0.5 ? KeyState.Up : KeyState.Down;
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    HorizontalInput = -1.0f;
                else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    HorizontalInput = 1.0f;
                else
                    HorizontalInput = 0.0f;

                JumpButton.state = Keyboard.GetState().IsKeyDown(Keys.Up) ? KeyState.Down : KeyState.Up;
                CrouchButton.state = Keyboard.GetState().IsKeyDown(Keys.Down) ? KeyState.Down : KeyState.Up;
                AttackButton.state = Keyboard.GetState().IsKeyDown(Keys.RightControl) ? KeyState.Down : KeyState.Up;
                CameraMode.state = Keyboard.GetState().IsKeyDown(Keys.NumPad5) ? KeyState.Up : KeyState.Down;
                CameraLeft.state = Keyboard.GetState().IsKeyDown(Keys.NumPad4) ? KeyState.Up : KeyState.Down;
                CameraRight.state = Keyboard.GetState().IsKeyDown(Keys.NumPad6) ? KeyState.Up : KeyState.Down;
                CameraUp.state = Keyboard.GetState().IsKeyDown(Keys.NumPad8) ? KeyState.Up : KeyState.Down;
                CameraDown.state = Keyboard.GetState().IsKeyDown(Keys.NumPad2) ? KeyState.Up : KeyState.Down;
            }
        }
    }
}
