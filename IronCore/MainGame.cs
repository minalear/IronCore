using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using IronCore.Utils;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace IronCore
{
    public class MainGame : Game
    {
        private ContentManager content;
        private ShapeRenderer renderer;

        private World world;

        private Body rocket;
        private Shape rocketShape;
        private Shape[] levelGeometry;

        private List<Bullet> bullets;
        private float bulletCounter = 0f;

        public MainGame() : base("IronCore - Minalear", 800, 450) { }

        public override void Initialize()
        {
            world = new World(new Vector2(0f, 9.8f));
            bullets = new List<Bullet>();
        }
        public override void LoadContent()
        {
            content = new ContentManager("Content/");
            renderer = new ShapeRenderer(content, Window.Width, Window.Height);

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, Window.Height / 2f, 0f));

            levelGeometry = content.LoadMap(world, "Maps/test_map.json");
            initRocket();
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState gpadState = GamePad.GetState(0);

            //Bullet delay counter
            bulletCounter += gameTime.FrameDelta;
            if (bulletCounter > 0.05f)
                bulletCounter = 0f;

            //Destroy older bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Lifetime += gameTime.FrameDelta;
                if (bullets[i].Lifetime > 5f)
                {
                    world.RemoveBody(bullets[i].BulletBody);
                    bullets.RemoveAt(i--);
                }
            }

            //Process player input
            if (gpadState.ThumbSticks.Left.LengthSquared > 0.01f)
            {
                Vector2 leftStick = gpadState.ThumbSticks.Left;
                rocket.Rotation = (float)Math.Atan2(leftStick.X, leftStick.Y);

                //Only apply horizontal thrust
                leftStick.Y = 0f;
                rocket.ApplyForce(leftStick / 6f);
            }
            if (gpadState.Buttons.A == ButtonState.Pressed)
            {
                float x = (float)Math.Sin(rocket.Rotation);
                float y = -(float)Math.Cos(rocket.Rotation);

                rocket.ApplyForce(new Vector2(0f, y) / 5f);
            }

            if (gpadState.Buttons.RightShoulder == ButtonState.Pressed && bulletCounter == 0f)
            {
                spawnBullet();
            }

            //Simulate world
            world.Step(0.01f);
        }
        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            renderer.Begin();

            const float cameraScale = 2f;
            Vector2 rocketPosition = ConvertUnits.ToDisplayUnits(rocket.Position);
            Vector2 cameraCenter = -rocketPosition + new Vector2(Window.Width / 2f, Window.Height / 2f) / cameraScale;

            renderer.SetCamera(
                Matrix4.CreateTranslation(cameraCenter.X, cameraCenter.Y, 0f) *
                Matrix4.CreateScale(cameraScale));

            //Draw world geometry
            renderer.SetTransform(Matrix4.Identity);
            foreach (Shape shape in levelGeometry)
                renderer.DrawShape(shape.VertexData, Color4.LimeGreen);

            //Draw bullets
            foreach (var bullet in bullets)
                renderer.DrawCircle(ConvertUnits.ToDisplayUnits(bullet.BulletBody.Position), 1f, 4, Color4.Orange);

            //Draw rocket
            renderer.SetTransform(
                Matrix4.CreateRotationZ(rocket.Rotation) *
                Matrix4.CreateTranslation(rocketPosition.X, rocketPosition.Y, 0f));
            renderer.DrawShape(rocketShape.VertexData, Color4.Red);

            renderer.End();
            Window.SwapBuffers();
        }

        private void initRocket()
        {
            const float height = 8f / 2f;
            const float width = 5f / 2f;

            Vertices vertices = new Vertices(3);
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(0f, -height)));
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(-width, height)));
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(width, height)));

            rocket = BodyFactory.CreatePolygon(world, vertices, 5f);
            rocket.BodyType = BodyType.Dynamic;
            rocket.Position = ConvertUnits.ToSimUnits(new Vector2(100f, 50f));
            rocket.AngularDamping = 300f;
            rocket.LinearDamping = 8f;
            rocket.UserData = "Player";

            rocketShape = new Shape(6);

            rocketShape.VertexData[0] = 0f;
            rocketShape.VertexData[1] = -height;

            rocketShape.VertexData[2] = -width;
            rocketShape.VertexData[3] = height;

            rocketShape.VertexData[4] = width;
            rocketShape.VertexData[5] = height;
        }
        private void spawnBullet()
        {
            Vector2 direction = new Vector2(
                (float)Math.Sin(rocket.Rotation),
                -(float)Math.Cos(rocket.Rotation));
            Vector2 velocity = direction / 500f;
            Vector2 position = direction * 6f + ConvertUnits.ToDisplayUnits(rocket.Position);

            Body bullet = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(1f), 1f);
            bullet.Position = ConvertUnits.ToSimUnits(position);
            bullet.FixedRotation = true;
            bullet.BodyType = BodyType.Dynamic;
            bullet.IsBullet = true;
            bullet.OnCollision += Bullet_OnCollision;
            bullet.ApplyLinearImpulse(velocity);
            bullet.CollidesWith = (Category.All ^ Category.Cat1);

            bullet.UserData = "Player_Bullet";

            bullets.Add(new Bullet() { BulletBody = bullet });
        }

        private bool Bullet_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                if (bullets[i].BulletBody.BodyId == fixtureA.Body.BodyId)
                {
                    bullets.RemoveAt(i);
                    fixtureA.Body.Dispose();
                }
            }

            return true;
        }
    }

    public class Bullet
    {
        public Body BulletBody;
        public float Lifetime;
    }
}
