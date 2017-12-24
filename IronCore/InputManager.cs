using System;
using OpenTK;
using OpenTK.Input;
using IronCore.Utils;

namespace IronCore
{
    public static class InputManager
    {
        //TODO: Create a powerful input manager that can hotswap between mouse/keyboard input based on last used inputs

        private static GamePadState gpadLastState, gpadThisState;
        private static KeyboardState keyLastState, keyThisState;
        private static MouseState mouseLastState, mouseThisState;
        private static Vector2 currentMousePos;

        private static bool disableMouseKeyInput = true;

        public static void Initialize()
        {
            gpadLastState = gpadThisState = GamePad.GetState(0);
            keyLastState = keyThisState = Keyboard.GetState();
            mouseLastState = mouseThisState = Mouse.GetState();
        }
        public static void UpdateInputStates()
        {
            gpadLastState = gpadThisState;
            keyLastState = keyThisState;
            mouseLastState = mouseThisState;

            gpadThisState = GamePad.GetState(0);
            keyThisState = Keyboard.GetState();
            mouseThisState = Mouse.GetState();
        }
        public static void UpdateMousePosition(int x, int y)
        {
            currentMousePos.X = x;
            currentMousePos.Y = y;
        }

        public static Vector2 GetMoveDirection()
        {
            //TODO: Exploit, right now input manager just combines inputs from gamepad/keyboard
            //Should hotswap and only utilize one input method at a time

            Vector2 total = Vector2.Zero;

            if (gpadThisState.ThumbSticks.Left.LengthSquared > 0.01f)
            {
                Vector2 left = gpadThisState.ThumbSticks.Left;
                left.Y = -left.Y; //Invert Y axis

                total += left;
            }

            if (!disableMouseKeyInput)
            {
                if (keyThisState.IsKeyDown(Key.A))
                    total.X = -1f;
                else if (keyThisState.IsKeyDown(Key.D))
                    total.X = 1f;

                if (keyThisState.IsKeyDown(Key.W))
                    total.Y = -1f;
                else if (keyThisState.IsKeyDown(Key.S))
                    total.Y = 1f;
            }

            return total;
        }
        public static Vector2 GetLookDirection(Vector2 center)
        {
            if (gpadThisState.ThumbSticks.Right.LengthSquared > 0.01f)
                return gpadThisState.ThumbSticks.Right;
            else if (disableMouseKeyInput)
                return Vector2.Zero;

            return (center - currentMousePos).SafeNormalize();
        }

        public static float LeftTrigger()
        {
            return gpadThisState.Triggers.Left;
        }
        public static float RightTrigger()
        {
            return gpadThisState.Triggers.Right;
        }
        public static float BoostMod()
        {
            if (keyThisState.IsKeyDown(Key.ShiftLeft) && !disableMouseKeyInput) return 2f;
            return RightTrigger() + 1f;
        }

        public static bool FirePrimary()
        {
            if (gpadThisState.Buttons.LeftShoulder == ButtonState.Pressed)
                return true;
            return (mouseThisState.LeftButton == ButtonState.Pressed && !disableMouseKeyInput);
        }
        public static bool FireSecondary()
        {
            if (gpadThisState.Buttons.RightShoulder == ButtonState.Pressed)
                return true;
            return (mouseThisState.RightButton == ButtonState.Pressed && !disableMouseKeyInput);
        }
    }
}
