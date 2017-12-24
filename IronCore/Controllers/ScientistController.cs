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
            Vector2 playerPosition = Entity.Map.Player.PhysicsBody.Position;
            float distToPlayer = playerPosition.DistanceSquared(Entity.PhysicsBody.Position);
            float playerSpeed = Entity.Map.Player.PhysicsBody.LinearVelocity.LengthSquared;

            if (distToPlayer > 0.1f || playerSpeed != 0f) //Patrol
            {
                Entity.Color = Color4.Aquamarine;

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
                Entity.Color = Color4.DarkGoldenrod;

                positionMod = (playerPosition.X < Entity.PhysicsBody.Position.X) ? -1f : 1f;
                positionTracker += SCIENTIST_SPEED * positionMod;

                //Board the player ship
                if (distToPlayer <= 0.002f)
                {
                    Entity.Map.ScientistCount--;
                    Entity.PurgeSelf();
                    InterfaceManager.UpdateUI = true;
                }
            }

            //Interpolate entity position between the bounds of the spawn area
            float y = Entity.DisplayPosition.Y;
            Entity.DisplayPosition = Vector2.Lerp(
                new Vector2(Entity.SpawnArea.Left, y),
                new Vector2(Entity.SpawnArea.Right, y), positionTracker);
            Entity.PhysicsBody.Position = ConvertUnits.ToSimUnits(Entity.DisplayPosition);
        }
    }
}
