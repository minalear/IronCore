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
            float distToPlayer = map.Player.PhysicsBody.Position.DistanceSquared(PhysicsBody.Position);
            if (distToPlayer > 50f) return;

            //TODO: Profile and optimize enemy->player detection
            var fixtureList =
                (from fixture in map.World.RayCast(PhysicsBody.Position, map.Player.PhysicsBody.Position)
                 where fixture.CollisionCategories == Category.Cat2
                 select fixture).ToList();
            if (fixtureList.Count <= 1)
                Color = Color4.Red;
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawCircle(Area, 24, Color);
        }

        public override void OnEntityCollision(Entity other)
        {
            //TODO: Change entities to use a unique id to cut down on type comparisons
            if (other.GetType() == typeof(Bullet))
            {
                Bullet bullet = ((Bullet)other);
                if (bullet.Owner.GetType() == typeof(Player))
                    CurrentHealth -= bullet.Damage;

                if (CurrentHealth <= 0f)
                {
                    PurgeSelf();
                    map.EnemyCount--;
                    InterfaceManager.UpdateUI = true;
                }
            }
        }
    }
}
