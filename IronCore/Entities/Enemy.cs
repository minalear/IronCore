using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics.Dynamics;
using IronCore.Utils;

namespace IronCore.Entities
{
    public class Enemy : Entity
    {
        public CircleF Area;
        public float CurrentHealth;

        public Color4 Color = Color4.Cyan;

        public Enemy(Map map) : base(map) { }

        public override void Update(GameTime gameTime)
        {
            Color = Color4.Cyan;
            float distToPlayer = map.PlayerBody.Position.DistanceSquared(PhysicsBody.Position);
            if (distToPlayer > 50f) return;

            //TODO: Profile and optimize enemy->player detection
            var fixtureList =
                (from fixture in map.World.RayCast(PhysicsBody.Position, map.PlayerBody.Position)
                 where fixture.CollisionCategories == Category.Cat2
                 select fixture).ToList();
            if (fixtureList.Count <= 1)
                Color = Color4.Red;
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawCircle(Area, 24, Color);
        }
    }
}
