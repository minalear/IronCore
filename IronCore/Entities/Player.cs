using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using IronCore.Utils;
using IronCore.Controllers;

namespace IronCore.Entities
{
    public class Player : Entity
    {
        private PlayerController controller;
        private StaticGeometry shape;
        private float health = 100f;
        private int ammoCount = 200;
        private float bulletCounter = 0f;

        public Player(Map map) : base(map)
        {
            const float height = 8f / 2f;
            const float width = 5f / 2f;

            Vertices vertices = new Vertices(3);
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(0f, -height)));
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(-width, height)));
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(width, height)));

            Body physicsBody = BodyFactory.CreatePolygon(map.World, vertices, 5f);
            physicsBody.BodyType = BodyType.Dynamic;
            physicsBody.Position = ConvertUnits.ToSimUnits(new Vector2(830f, 298f));
            physicsBody.AngularDamping = 300f;
            physicsBody.LinearDamping = 8f;
            physicsBody.OnCollision += Rocket_OnCollision;

            shape = new StaticGeometry(6);
            shape.PhysicsBody = physicsBody;

            shape.VertexData[0] = 0f;
            shape.VertexData[1] = -height;

            shape.VertexData[2] = -width;
            shape.VertexData[3] = height;

            shape.VertexData[4] = width;
            shape.VertexData[5] = height;

            SetPhysicsBody(physicsBody);
            controller = new PlayerController(this);
        }

        public override void Update(GameTime gameTime)
        {
            bulletCounter += gameTime.FrameDelta;
            controller.Update(gameTime);
        }
        public override void Draw(ShapeRenderer renderer)
        {
            Vector2 position = ConvertUnits.ToDisplayUnits(physicsBody.Position);
            renderer.SetTransform(
                Matrix4.CreateRotationZ(PhysicsBody.Rotation) *
                Matrix4.CreateTranslation(position.X, position.Y, 0f));
            renderer.DrawShape(shape.VertexData, ColorUtils.Blend(Color4.Red, Color4.Green, health / 100f));
            renderer.ClearTransform();
        }

        public void FirePrimary()
        {
            if (bulletCounter < 0.08f || ammoCount <= 0) return;

            Vector2 direction = new Vector2(
                (float)Math.Sin(physicsBody.Rotation),
                -(float)Math.Cos(physicsBody.Rotation));
            Vector2 velocity = direction / 500f;
            Vector2 position = direction * 6f + ConvertUnits.ToDisplayUnits(physicsBody.Position);

            map.SpawnBullet(this, position, velocity, 1f, 1f);

            //Apply inverse force to rocket
            physicsBody.ApplyForce(-velocity);
            ammoCount -= 1;
            bulletCounter = 0f;
        }
        public void FireSecondary()
        {
            if (bulletCounter < 0.08f || ammoCount <= 1) return;

            Vector2 direction = new Vector2(
                (float)Math.Sin(physicsBody.Rotation),
                -(float)Math.Cos(physicsBody.Rotation));

            Vector2 left = new Vector2(
                 (float)Math.Sin(physicsBody.Rotation - MathHelper.PiOver6),
                -(float)Math.Cos(physicsBody.Rotation - MathHelper.PiOver6));
            Vector2 right = new Vector2(
                 (float)Math.Sin(physicsBody.Rotation + MathHelper.PiOver6),
                -(float)Math.Cos(physicsBody.Rotation + MathHelper.PiOver6));

            Vector2 position = direction * 6f + ConvertUnits.ToDisplayUnits(physicsBody.Position);

            map.SpawnBullet(this, position, left / 250f, 2f, 2f);
            map.SpawnBullet(this, position, right / 250f, 2f, 2f);

            //Apply inverse force to rocket
            physicsBody.ApplyForce(-left / 500f);
            physicsBody.ApplyForce(-right / 500f);
            ammoCount -= 2;
            bulletCounter = 0f;
        }

        private bool Rocket_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.Body.UserData == null) return true;

            if (fixtureB.Body.UserData.Equals("Level"))
            {
                float velLength = physicsBody.LinearVelocity.Length;
                if (velLength > 1f) //Deal damage
                {
                    float damage = 1f * velLength;
                    health -= damage;

                    if (health <= 0f)
                        health = 0f;
                    InterfaceManager.UpdateUI = true;
                }
            }
            
            return true;
        }

        public float Health { get { return health; } set { health = value; } }
        public int AmmoCount { get { return ammoCount; } set { ammoCount = value; } }
    }
}
