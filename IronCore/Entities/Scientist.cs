using System;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics;
using IronCore.Utils;

namespace IronCore.Entities
{
    public class Scientist : Entity
    {
        public Vector2 DisplayPosition;
        public RectangleF SpawnArea;
        public Color4 Color = Color4.Aquamarine;

        private float positionTracker;
        private float positionMod = 1f;

        public Scientist(Map map, Vector2 displayPosition, RectangleF spawnArea) : base(map)
        {
            DisplayPosition = displayPosition;
            SpawnArea = spawnArea;

            //Position between the left and right boundary of the spawn area
            positionTracker = (displayPosition.X - spawnArea.X) / spawnArea.Width;
        }

        public override void Update(GameTime gameTime)
        {
            float distToPlayer = map.PlayerBody.Position.DistanceSquared(PhysicsBody.Position);
            float playerVel = map.PlayerBody.LinearVelocity.LengthSquared;

            if (distToPlayer > 0.1f || playerVel != 0f) //Patrol
            {
                Color = Color4.Aquamarine;

                positionTracker += 0.003f * positionMod;
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
            else if (playerVel == 0f)
            {
                Color = Color4.DarkGoldenrod;

                positionMod = (map.PlayerBody.Position.X < PhysicsBody.Position.X) ? -1f : 1f;
                positionTracker += 0.003f * positionMod;

                if (distToPlayer <= 0.002f)
                {
                    DoPurge = true;
                }
            }

            float y = DisplayPosition.Y;
            DisplayPosition = Vector2.Lerp(new Vector2(SpawnArea.Left, y), new Vector2(SpawnArea.Right, y), positionTracker);
            PhysicsBody.Position = ConvertUnits.ToSimUnits(DisplayPosition);
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawCircle(DisplayPosition, 1f, 6, Color);
        }
    }
}
