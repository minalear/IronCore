using System;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using IronCore.Utils;
using IronCore.Entities;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

namespace IronCore.Controllers
{
    public class TurretController : Controller<Enemy>
    {
        public Vector2 DirectionToPlayer;

        private float fireTimer = 0f;

        public TurretController(Enemy enemy) : base(enemy) { }

        public override void Update(GameTime gameTime)
        {
            Parent.Color = Color4.Cyan;
            float distToPlayer = Parent.Map.Player.PhysicsBody.Position.DistanceSquared(Parent.PhysicsBody.Position);
            if (distToPlayer > 15f) return;

            DirectionToPlayer = Parent.Map.Player.PhysicsBody.Position - Parent.PhysicsBody.Position;
            DirectionToPlayer.Normalize();

            //TODO: Profile and optimize enemy->player detection
            var fixtureList =
                (from fixture in Parent.Map.World.RayCast(Parent.PhysicsBody.Position, Parent.Map.Player.PhysicsBody.Position)
                 where fixture.CollisionCategories == Category.Cat2
                 select fixture).ToList();
            if (fixtureList.Count <= 1)
            {
                Parent.Color = Color4.Red;

                fireTimer += gameTime.FrameDelta;
                if (fireTimer >= 1f)
                {
                    fireTimer = 0f;
                    Parent.Map.SpawnBullet(Parent,
                        Parent.Area.Position + DirectionToPlayer * (Parent.Area.Radius + 6f),
                        DirectionToPlayer / 10f, 8f, 1f);
                }
            }
        }
    }
}
