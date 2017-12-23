using System;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics;
using IronCore.Utils;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace IronCore.Entities
{
    public class Bullet : Entity
    {
        public Entity Owner;
        public float Lifetime;
        public float Damage;

        public Bullet(Map map, Entity owner, float damage) : base(map)
        {
            Owner = owner;
            Damage = damage;
        }

        public override void Update(GameTime gameTime)
        {
            Lifetime += gameTime.FrameDelta;
            if (Lifetime > 5f)
            {
                PurgeSelf();
            }
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawCircle(ConvertUnits.ToDisplayUnits(physicsBody.Position), 1f, 4, Color4.OrangeRed);
        }

        protected override bool physicsBodyOnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            PurgeSelf();
            return base.physicsBodyOnCollision(fixtureA, fixtureB, contact);
        }
    }
}
