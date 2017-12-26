using System.Text;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using IronCore.Utils;

namespace IronCore
{
    public static class InputManager
    {
        //TODO: Create a powerful input manager that can hotswap between mouse/keyboard input based on last used inputs

        //TODO: Figure out a way to utilize only GamePadState or JoystickState
        private static KeyboardState keyLastState, keyThisState;
        private static MouseState mouseLastState, mouseThisState;
        private static JoystickState joyLastState, joyThisState;
        private static Vector2 currentMousePos;

        private static Dictionary<Buttons, int> gamepadButtonIDs;

        private static bool disableMouseKeyInput = true;

        public static void Initialize()
        {
            keyLastState = keyThisState = Keyboard.GetState();
            mouseLastState = mouseThisState = Mouse.GetState();
            joyLastState = joyThisState = Joystick.GetState(0);

            //IDs based on experimenting with DS4 
            //TODO: Check Xbox 360 controller to ensure similar bindings
            gamepadButtonIDs = new Dictionary<Buttons, int>();
            gamepadButtonIDs.Add(Buttons.A, 1); //DS4 Cross (X)
            gamepadButtonIDs.Add(Buttons.B, 2); //DS4 Circle
            gamepadButtonIDs.Add(Buttons.X, 0); //DS4 Square
            gamepadButtonIDs.Add(Buttons.Y, 3); //DS4 Triangle
            gamepadButtonIDs.Add(Buttons.LeftShoulder, 4); //LB
            gamepadButtonIDs.Add(Buttons.RightShoulder, 5); //RB
            gamepadButtonIDs.Add(Buttons.LeftTrigger, 6);
            gamepadButtonIDs.Add(Buttons.RightTrigger, 7);
            gamepadButtonIDs.Add(Buttons.LeftStick, 10);
            gamepadButtonIDs.Add(Buttons.RightStick, 11);
            gamepadButtonIDs.Add(Buttons.Back, 8); //Select - DS4 Share
            gamepadButtonIDs.Add(Buttons.Start, 9); //DS4 - Options
            gamepadButtonIDs.Add(Buttons.BigButton, 12); //Guide
            //TODO: Implement Touchpad (Both sides ID = 13)

            //Joystick Axis IDs
            //Left  (XY) = 01
            //Right (XY) = 23
            //L Trg      =  4
            //R Trg      =  5
        }
        public static void UpdateInputStates()
        {
            keyLastState = keyThisState;
            mouseLastState = mouseThisState;
            joyLastState = joyThisState;
            
            keyThisState = Keyboard.GetState();
            mouseThisState = Mouse.GetState();
            joyThisState = Joystick.GetState(0);
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
            int buttonID = gamepadButtonIDs[button];
            return (joyThisState.IsButtonDown(buttonID) && joyLastState.IsButtonUp(buttonID));
        }
        public static bool IsButtonReleased(Buttons button)
        {
            int buttonID = gamepadButtonIDs[button];
            return (joyThisState.IsButtonUp(buttonID) && joyLastState.IsButtonDown(buttonID));
        }
        public static bool IsButtonUp(Buttons button)
        {
            return joyThisState.IsButtonUp(gamepadButtonIDs[button]);
        }
        public static bool IsButtonDown(Buttons button)
        {
            return joyThisState.IsButtonDown(gamepadButtonIDs[button]);
        }

        public static bool IsDpadButtonPressed(Buttons buttons)
        {
            var thisHatState = joyThisState.GetHat(JoystickHat.Hat0);
            var lastHatState = joyLastState.GetHat(JoystickHat.Hat0);

            if (buttons == Buttons.DPadUp)
                return thisHatState.IsUp && !lastHatState.IsUp;
            if (buttons == Buttons.DPadDown)
                return thisHatState.IsDown && !lastHatState.IsDown;
            if (buttons == Buttons.DPadLeft)
                return thisHatState.IsLeft && !lastHatState.IsLeft;
            if (buttons == Buttons.DPadRight)
                return thisHatState.IsRight && !lastHatState.IsRight;

            return false;
        }
        public static bool IsDpadButtonReleased(Buttons buttons)
        {
            var thisHatState = joyThisState.GetHat(JoystickHat.Hat0);
            var lastHatState = joyLastState.GetHat(JoystickHat.Hat0);

            if (buttons == Buttons.DPadUp)
                return !thisHatState.IsUp && lastHatState.IsUp;
            if (buttons == Buttons.DPadDown)
                return !thisHatState.IsDown && lastHatState.IsDown;
            if (buttons == Buttons.DPadLeft)
                return !thisHatState.IsLeft && lastHatState.IsLeft;
            if (buttons == Buttons.DPadRight)
                return !thisHatState.IsRight && lastHatState.IsRight;

            return false;
        }

        public static Vector2 LeftStick()
        {
            float x = joyThisState.GetAxis(LEFT_STICK_X_ID);
            float y = joyThisState.GetAxis(LEFT_STICK_Y_ID);

            return new Vector2(x, y);
        }
        public static Vector2 RightStick()
        {
            float x = joyThisState.GetAxis(RIGHT_STICK_X_ID);
            float y = joyThisState.GetAxis(RIGHT_STICK_Y_ID);

            return new Vector2(x, y);
        }
        public static float LeftTrigger()
        {
            //JoystickAxis for triggers go from -1 to 1 (expecting 0 to 1)
            return (joyThisState.GetAxis(LEFT_TRIGGER_ID) + 1f) / 2f;
        }
        public static float RightTrigger()
        {
            //JoystickAxis for triggers go from -1 to 1 (expecting 0 to 1)
            return (joyThisState.GetAxis(RIGHT_TRIGGER_ID) + 1f) / 2f;
        }

        public static Vector2 GetMoveDirection()
        {
            //TODO: Exploit, right now input manager just combines inputs from gamepad/keyboard
            //Should hotswap and only utilize one input method at a time

            Vector2 total = Vector2.Zero;
            Vector2 leftStick = LeftStick();

            if (leftStick.LengthSquared > 0.01f)
            {
                //leftStick.Y = -leftStick.Y; //Invert Y axis
                //GamepadState thumbsticks Y axis is inverted compared to Joystick axis

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

        private const JoystickAxis  LEFT_STICK_X_ID = (JoystickAxis)0;
        private const JoystickAxis  LEFT_STICK_Y_ID = (JoystickAxis)1;
        private const JoystickAxis RIGHT_STICK_X_ID = (JoystickAxis)2;
        private const JoystickAxis RIGHT_STICK_Y_ID = (JoystickAxis)3;
        private const JoystickAxis  LEFT_TRIGGER_ID = (JoystickAxis)4;
        private const JoystickAxis RIGHT_TRIGGER_ID = (JoystickAxis)5;
    }
}
