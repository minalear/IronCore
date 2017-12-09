using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;

namespace IronCore.Physics
{
    public class World
    {
        private List<Particle> particles;

        public World()
        {
            particles = new List<Particle>();
            particles.Add(new Particle());
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].ApplyForce(Gravity * particles[i].Mass);
                particles[i].Integrate(gameTime.FrameDelta);
            }

            
        }
        public void Draw(ShapeRenderer renderer)
        {
            renderer.Begin();
            for (int i = 0; i < particles.Count; i++)
                renderer.DrawPixel(particles[i].Position, Color4.White);
            renderer.End();
        }

        public const float GRAVITY_CONSTANT = 8f;
        public static Vector2 Gravity = new Vector2(0f, 1f) * GRAVITY_CONSTANT;
    }
}
