using System;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using IronCore.Utils;

namespace IronCore
{
    using ButtonSelector = Func<GamePadState, ButtonState>;

    public static class InputManager
    {
        //TODO: Create a powerful input manager that can hotswap between mouse/keyboard input based on last used inputs

        //TODO: Figure out a way to utilize only GamePadState or JoystickState
        private static KeyboardState keyLastState, keyThisState;
        private static MouseState mouseLastState, mouseThisState;
        private static GamePadState padLastState, padThisState;
        private static Vector2 currentMousePos;

        //Used to access GamePad's button state cleanly.
        private static Dictionary<Buttons, ButtonSelector> buttonSelectors;

        private static bool disableMouseKeyInput = true;
        
        public static void Initialize()
        {
            keyLastState = keyThisState = Keyboard.GetState();
            mouseLastState = mouseThisState = Mouse.GetState();
            padLastState = padThisState = GamePad.GetState(0);

            buttonSelectors = new Dictionary<Buttons, ButtonSelector>()
            {
                { Buttons.A, state => state.Buttons.A }, //A
                { Buttons.B, state => state.Buttons.B }, //B
                { Buttons.X, state => state.Buttons.X }, //X
                { Buttons.Y, state => state.Buttons.Y }, //Y

                { Buttons.LeftShoulder, state => state.Buttons.LeftShoulder }, //Left Bumper
                { Buttons.RightShoulder, state => state.Buttons.RightShoulder }, //Right Bumper

                //{ Buttons.A, state => state.Buttons.A }, //Left Trigger
                //{ Buttons.A, state => state.Buttons.A }, //Right Trigger

                { Buttons.LeftStick, state => state.Buttons.LeftStick }, //Left Stick
                { Buttons.RightStick, state => state.Buttons.RightStick }, //Right Stick

                { Buttons.Back, state => state.Buttons.Back }, //Back/Select
                { Buttons.Start, state => state.Buttons.Start }, //Start

                { Buttons.BigButton, state => state.Buttons.BigButton } //Home Button
            };
        }
        public static void UpdateInputStates()
        {
            keyLastState = keyThisState;
            mouseLastState = mouseThisState;
            padLastState = padThisState;
            
            keyThisState = Keyboard.GetState();
            mouseThisState = Mouse.GetState();
            padThisState = GamePad.GetState(0);
        }

        //TODO: Replace methods like this with event triggers from Window
        public static void UpdateMousePosition(int x, int y)
        {
            currentMousePos.X = x;
            currentMousePos.Y = y;
        }

        //TODO: Consider changing function names to be more representative of the state they are testing
        //TODO: These throw errors when passing DpadButtons.  Consider creating our own enum
        public static bool IsButtonPressed(Buttons button)
        {
            return (buttonSelectors[button].Invoke(padThisState) == ButtonState.Pressed) &&
                   (buttonSelectors[button].Invoke(padLastState) == ButtonState.Released);
        }
        public static bool IsButtonReleased(Buttons button)
        {
            return (buttonSelectors[button].Invoke(padThisState) == ButtonState.Pressed) &&
                   (buttonSelectors[button].Invoke(padLastState) == ButtonState.Released);
        }
        public static bool IsButtonUp(Buttons button)
        {
            return (buttonSelectors[button].Invoke(padThisState) == ButtonState.Released);
        }
        public static bool IsButtonDown(Buttons button)
        {
            return (buttonSelectors[button].Invoke(padThisState) == ButtonState.Pressed);
        }

        public static bool IsDpadButtonPressed(Buttons buttons)
        {
            if (buttons == Buttons.DPadUp)
                return padThisState.DPad.IsUp && !padLastState.DPad.IsUp;
            if (buttons == Buttons.DPadDown)
                return padThisState.DPad.IsDown && !padLastState.DPad.IsDown;
            if (buttons == Buttons.DPadLeft)
                return padThisState.DPad.IsLeft && !padLastState.DPad.IsLeft;
            if (buttons == Buttons.DPadRight)
                return padThisState.DPad.IsRight && !padLastState.DPad.IsRight;

            return false;
        }
        public static bool IsDpadButtonReleased(Buttons buttons)
        {
            if (buttons == Buttons.DPadUp)
                return !padThisState.DPad.IsUp && padLastState.DPad.IsUp;
            if (buttons == Buttons.DPadDown)
                return !padThisState.DPad.IsDown && padLastState.DPad.IsDown;
            if (buttons == Buttons.DPadLeft)
                return !padThisState.DPad.IsLeft && padLastState.DPad.IsLeft;
            if (buttons == Buttons.DPadRight)
                return !padThisState.DPad.IsRight && padLastState.DPad.IsRight;

            return false;
        }

        public static Vector2 LeftStick()
        {
            Vector2 leftStick = padThisState.ThumbSticks.Left;
            leftStick.Y = -leftStick.Y; //Invert Y

            return leftStick;
        }
        public static Vector2 RightStick()
        {
            Vector2 rightSTick = padThisState.ThumbSticks.Right;
            rightSTick.Y = -rightSTick.Y; //Invert Y

            return rightSTick;
        }
        public static float LeftTrigger()
        {
            return padThisState.Triggers.Left;
        }
        public static float RightTrigger()
        {
            return padThisState.Triggers.Right;
        }

        public static Vector2 GetMoveDirection()
        {
            //TODO: Exploit, right now input manager just combines inputs from gamepad/keyboard
            //Should hotswap and only utilize one input method at a time

            Vector2 total = Vector2.Zero;
            Vector2 leftStick = LeftStick();

            if (leftStick.LengthSquared > 0.01f)
            {
                total += leftStick;
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
            Vector2 rightStick = RightStick();
            if (rightStick.LengthSquared > 0.01f)
                return rightStick;
            else if (disableMouseKeyInput)
                return Vector2.Zero;

            return (center - currentMousePos).SafeNormalize();
        }

        //TODO: Implement input events for buttons
        
        public static float BoostMod()
        {
            if (keyThisState.IsKeyDown(Key.ShiftLeft) && !disableMouseKeyInput) return 2f;
            return RightTrigger() + 1f;
        }

        public static bool FirePrimary()
        {
            if (IsButtonDown(Buttons.RightShoulder))
                return true;
            return (mouseThisState.LeftButton == ButtonState.Pressed && !disableMouseKeyInput);
        }
        public static bool FireSecondary()
        {
            if (IsButtonDown(Buttons.LeftShoulder))
                return true;
            return (mouseThisState.RightButton == ButtonState.Pressed && !disableMouseKeyInput);
        }
    }
}
