using System;
using OpenTK;
using IronCore.Utils;

namespace IronCore.Physics
{
    public class Particle
    {
        private World world;

        private Vector2 position;
        private Vector2 velocity;
        private Vector2 acceleration;

        private float inverseMass; //1 / mass
        private float damping;

        public void UpdatePosition(GameTime gameTime)
        {
            position += velocity * gameTime.FrameDelta;
            position += acceleration * (gameTime.FrameDelta * gameTime.FrameDelta) / 2f;
        }
        public void UpdateVelocity(GameTime gameTime)
        {
            Vector2 gravity = 5f * new Vector2(0f, 1f);
            acceleration += gravity;

            velocity += acceleration * gameTime.FrameDelta;
            velocity *= (float)Math.Pow(damping, gameTime.FrameDelta); //Damping
        }

        public Vector2 Position { get { return position; } }
        public Vector2 Velocity { get { return velocity; } }
        public Vector2 Acceleration { get { return acceleration; } } 
    }
}
