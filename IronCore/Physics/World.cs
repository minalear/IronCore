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
                particles[i].UpdateVelocity(gameTime);
                particles[i].UpdatePosition(gameTime);
            }
        }
        public void Draw(ShapeRenderer renderer)
        {
            renderer.Begin();
            for (int i = 0; i < particles.Count; i++)
                renderer.DrawPixel(particles[i].Position, Color4.White);
            renderer.End();
        }
    }
}
