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

        private float inverseMass = 1f; //1 / mass
        private float damping = 0.9999f;

        public void Integrate(float timestep)
        {
            //Return out if mass is infinite
            if (inverseMass <= 0f) return;

            position += velocity * timestep;

            //Calculate forces
            acceleration += World.Gravity;
            velocity += acceleration * timestep;

            //Impose drag
            velocity *= (float)Math.Pow(damping, timestep);
        }

        public Vector2 Position { get { return position; } }
        public Vector2 Velocity { get { return velocity; } }
        public Vector2 Acceleration { get { return acceleration; } } 
    }
}
