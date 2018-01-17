using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics;
using IronCore.Utils;
using IronCore.Controllers;

namespace IronCore.Entities
{
    public class Enemy : Entity
    {
        public CircleF Area;
        public float CurrentHealth;

        public Color4 Color = Color4.Cyan;

        private TurretController controller;

        public Enemy(Map map) : base(map)
        {
            controller = new TurretController(this);
        }

        public override void Update(GameTime gameTime)
        {
            controller.Update(gameTime);
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawCircle(Area, 24, Color);

            Vector2 toPlayer = ConvertUnits.ToDisplayUnits(controller.DirectionToPlayer);
            //renderer.DrawLine(Area.Position, Area.Position + toPlayer, Color4.Red);
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
                    InterfaceManager.RefreshUI = true;
                }
            }
        }
    }
}
