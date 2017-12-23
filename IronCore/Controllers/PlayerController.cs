using System;
using OpenTK;
using OpenTK.Input;
using IronCore.Utils;
using IronCore.Entities;

namespace IronCore.Controllers
{
    public class PlayerController : Controller<Player>
    {
        public PlayerController(Player player) : base(player) { }

        public override void Update(GameTime gameTime)
        {
            var gpadState = GamePad.GetState(0);

            //Movement
            if (gpadState.ThumbSticks.Left.LengthSquared > 0.01f)
            {
                Vector2 leftStick = gpadState.ThumbSticks.Left;
                parent.PhysicsBody.Rotation = (float)Math.Atan2(leftStick.X, leftStick.Y);

                //Only apply horizontal thrust
                leftStick.Y = -leftStick.Y;

                float mod = gpadState.Triggers.Right + 1f;
                Vector2 force = leftStick * mod / 5f;
                parent.PhysicsBody.ApplyForce(leftStick * mod / 5f);
            }
            if (gpadState.ThumbSticks.Right.LengthSquared > 0.01f)
            {
                Vector2 rightStick = gpadState.ThumbSticks.Right;
                parent.PhysicsBody.Rotation = (float)Math.Atan2(rightStick.X, rightStick.Y);
            }

            //Guns
            if (gpadState.Buttons.RightShoulder == ButtonState.Pressed)
            {
                parent.FirePrimary();
                InterfaceManager.UpdateUI = true;
            }
            else if (gpadState.Buttons.LeftShoulder == ButtonState.Pressed)
            {
                parent.FireSecondary();
                InterfaceManager.UpdateUI = true;
            }
        }
    }
}
