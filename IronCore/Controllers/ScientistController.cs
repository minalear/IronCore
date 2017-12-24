using System;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;
using IronCore.Entities;
using FarseerPhysics;

namespace IronCore.Controllers
{
    public class ScientistController : Controller<Scientist>
    {
        private float positionTracker;
        private float positionMod = 1f;

        private const float SCIENTIST_SPEED = 0.003f;

        public ScientistController(Scientist scientist) : base(scientist)
        {
            positionTracker = scientist.DisplayPosition.X - scientist.SpawnArea.X;
            positionTracker /= scientist.SpawnArea.Width;
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 playerPosition = Parent.Map.Player.PhysicsBody.Position;
            float distToPlayer = playerPosition.DistanceSquared(Parent.PhysicsBody.Position);
            float playerSpeed = Parent.Map.Player.PhysicsBody.LinearVelocity.LengthSquared;

            if (distToPlayer > 0.1f || playerSpeed != 0f) //Patrol
            {
                Parent.Color = Color4.Aquamarine;

                //Bounce between 0 and 1
                positionTracker += SCIENTIST_SPEED * positionMod;
                if (positionTracker >= 1f)
                {
                    positionTracker = 1f;
                    positionMod = -1f;
                }
                else if (positionTracker <= 0f)
                {
                    positionMod = 0f;
                    positionMod = 1f;
                }
            }
            else if (playerSpeed == 0f) //Approach player ship
            {
                Parent.Color = Color4.DarkGoldenrod;

                positionMod = (playerPosition.X < Parent.PhysicsBody.Position.X) ? -1f : 1f;
                positionTracker += SCIENTIST_SPEED * positionMod;

                //Board the player ship
                if (distToPlayer <= 0.002f)
                {
                    Parent.Map.ScientistCount--;
                    Parent.PurgeSelf();
                    InterfaceManager.UpdateUI = true;
                }
            }

            //Interpolate entity position between the bounds of the spawn area
            float y = Parent.DisplayPosition.Y;
            Parent.DisplayPosition = Vector2.Lerp(
                new Vector2(Parent.SpawnArea.Left, y),
                new Vector2(Parent.SpawnArea.Right, y), positionTracker);
            Parent.PhysicsBody.Position = ConvertUnits.ToSimUnits(Parent.DisplayPosition);
        }
    }
}
