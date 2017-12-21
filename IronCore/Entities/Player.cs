using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using IronCore.Utils;

namespace IronCore.Entities
{
    public class Player : Entity
    {
        private StaticGeometry shape;
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

            physicsBody = BodyFactory.CreatePolygon(map.World, vertices, 5f);
            physicsBody.BodyType = BodyType.Dynamic;
            physicsBody.Position = ConvertUnits.ToSimUnits(new Vector2(830f, 298f));
            physicsBody.AngularDamping = 300f;
            physicsBody.LinearDamping = 8f;
            physicsBody.UserData = "Player";
            physicsBody.OnCollision += Rocket_OnCollision;

            shape = new StaticGeometry(6);
            shape.PhysicsBody = physicsBody;

            shape.VertexData[0] = 0f;
            shape.VertexData[1] = -height;

            shape.VertexData[2] = -width;
            shape.VertexData[3] = height;

            shape.VertexData[4] = width;
            shape.VertexData[5] = height;
        }

        public override void Update(GameTime gameTime)
        {
            var gpadState = GamePad.GetState(0);

            //Movement
            if (gpadState.ThumbSticks.Left.LengthSquared > 0.01f)
            {
                Vector2 leftStick = gpadState.ThumbSticks.Left;
                PhysicsBody.Rotation = (float)Math.Atan2(leftStick.X, leftStick.Y);

                //Only apply horizontal thrust
                leftStick.Y = -leftStick.Y;

                float mod = gpadState.Triggers.Right + 1f;
                Vector2 force = leftStick * mod / 5f;
                PhysicsBody.ApplyForce(leftStick * mod / 5f);
            }
            if (gpadState.ThumbSticks.Right.LengthSquared > 0.01f)
            {
                Vector2 rightStick = gpadState.ThumbSticks.Right;
                PhysicsBody.Rotation = (float)Math.Atan2(rightStick.X, rightStick.Y);
            }

            //Guns
            bulletCounter += gameTime.FrameDelta;

            if (gpadState.Buttons.RightShoulder == ButtonState.Pressed && bulletCounter > 0.08f)
            {
                firePrimary();
                bulletCounter = 0f;
            }
            else if (gpadState.Buttons.LeftShoulder == ButtonState.Pressed && bulletCounter > 0.08f)
            {
                fireSecondary();
                bulletCounter = 0f;
            }
        }
        public override void Draw(ShapeRenderer renderer)
        {
            Vector2 position = ConvertUnits.ToDisplayUnits(physicsBody.Position);
            renderer.SetTransform(
                Matrix4.CreateRotationZ(PhysicsBody.Rotation) *
                Matrix4.CreateTranslation(position.X, position.Y, 0f));
            renderer.DrawShape(shape.VertexData, Color4.GreenYellow);
            renderer.ClearTransform();
        }

        private void firePrimary()
        {
            Vector2 direction = new Vector2(
                (float)Math.Sin(physicsBody.Rotation),
                -(float)Math.Cos(physicsBody.Rotation));
            Vector2 velocity = direction / 500f;
            Vector2 position = direction * 6f + ConvertUnits.ToDisplayUnits(physicsBody.Position);

            map.SpawnBullet(this, position, velocity, 1f, 1f);

            //Apply inverse force to rocket
            physicsBody.ApplyForce(-velocity);
        }
        private void fireSecondary()
        {
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
        }

        private bool Rocket_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.Body.UserData == null) return true;

            if (fixtureB.Body.UserData.Equals("Level"))
            {
                /*float velLength = rocket.LinearVelocity.Length;
                if (velLength > 1f) //Deal damage
                {
                    float damage = 1f * velLength;
                    playerHealth -= damage;

                    if (playerHealth <= 0f)
                        playerHealth = 0f;

                    Window.Title = playerHealth.ToString();
                }*/
            }

            //updateUI();
            return true;
        }
    }
}
