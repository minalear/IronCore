using System;
using OpenTK;

namespace IronCore.Physics
{
    public class Particle
    {
        private World world;

        private Vector2 position;
        private Vector2 velocity;
        private Vector2 acceleration;

        private Vector2 forceAccumulator;

        private float inverseMass = 1f; //1 / mass
        private float damping = 0.9999f;

        public void Integrate(float timestep)
        {
            //Return out if mass is infinite
            if (inverseMass <= 0f) return;

            //Update linear position
            position += velocity * timestep;

            //Calculate forces
            Vector2 currentAcceleration = acceleration;
            currentAcceleration += forceAccumulator * inverseMass;
            forceAccumulator = Vector2.Zero; //Clear accumulator

            //Update linear velocity
            velocity += acceleration * timestep;

            //Impose drag
            velocity *= (float)Math.Pow(damping, timestep);
        }

        public void ApplyForce(Vector2 force)
        {
            forceAccumulator += force;
        }

        public Vector2 Position { get { return position; } }
        public Vector2 Velocity { get { return velocity; } }
        public Vector2 Acceleration { get { return acceleration; } }
        public float Mass
        {
            get
            {
                if (inverseMass == 0f)
                    return float.MaxValue;
                return 1f / inverseMass;
            }
        } 
        public float InverseMass { get { return inverseMass; } }
    }
}
