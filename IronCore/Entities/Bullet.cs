using System;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics;
using IronCore.Utils;

namespace IronCore.Entities
{
    public class Bullet : Entity
    {
        public float Lifetime;
        public float Damage;

        public Bullet(Map map, float damage) : base(map)
        {
            Damage = damage;
        }

        public override void Update(GameTime gameTime)
        {
            Lifetime += gameTime.FrameDelta;
            if (Lifetime > 5f)
            {
                DoPurge = true;
            }
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawCircle(ConvertUnits.ToDisplayUnits(physicsBody.Position), 1f, 4, Color4.OrangeRed);
        }
    }
}
