using System;
using OpenTK;
using OpenTK.Input;
using IronCore.Utils;
using IronCore.Entities;
using FarseerPhysics;

namespace IronCore.Controllers
{
    public class PlayerController : Controller<Player>
    {
        public PlayerController(Player player) : base(player) { }

        public override void Update(GameTime gameTime)
        {
            //Movement
            Vector2 moveDirection = InputManager.GetMoveDirection();
            if (moveDirection.LengthSquared > 0f)
            {
                parent.PhysicsBody.Rotation = (float)Math.Atan2(moveDirection.Y, moveDirection.X) + MathHelper.PiOver2;

                float mod = InputManager.BoostMod();
                parent.PhysicsBody.ApplyForce(moveDirection * mod / 5f);
            }
            
            //Looking
            Vector2 lookDirection = InputManager.GetLookDirection(new Vector2(400f, 225f));
            if (lookDirection.LengthSquared > 0f)
            {
                parent.PhysicsBody.Rotation = (float)Math.Atan2(lookDirection.Y, lookDirection.X) + MathHelper.PiOver2;
            }

            //Guns
            if (InputManager.FirePrimary())
            {
                parent.FirePrimary();
                InterfaceManager.UpdateUI = true;
            }
            else if (InputManager.FireSecondary())
            {
                parent.FireSecondary();
                InterfaceManager.UpdateUI = true;
            }
        }
    }
}
