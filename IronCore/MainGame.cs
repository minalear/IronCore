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

        private InterfaceManager interfaceManager;

        private World world;

        private Body rocket;
        private StaticGeometry rocketShape;
        private Map map;

        private List<Bullet> bullets;
        private float bulletCounter = 0f;
        private float playerHealth = 100f;
        private int bulletCount = 200;

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
            interfaceManager = new InterfaceManager(content);

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, Window.Height / 2f, 0f));
            
            map = content.LoadMap(world, "Maps/physics_map.json");
            map.ScientistRetrieved += Map_ScientistRetrieved;

            initRocket();
            updateUI();
            map.PlayerBody = rocket;
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState gpadState = GamePad.GetState(0);

            //Bullet delay counter
            bulletCounter += gameTime.FrameDelta;
            if (bulletCounter > 0.08f)
                bulletCounter = 0f;

            //Destroy older bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Lifetime += gameTime.FrameDelta;
                if (bullets[i].Lifetime > 5f)
                {
                    world.RemoveBody(bullets[i].Body);
                    bullets.RemoveAt(i--);
                }
            }

            //Process player input
            if (gpadState.ThumbSticks.Left.LengthSquared > 0.01f)
            {
                Vector2 leftStick = gpadState.ThumbSticks.Left;
                rocket.Rotation = (float)Math.Atan2(leftStick.X, leftStick.Y);

                //Only apply horizontal thrust
                leftStick.Y = -leftStick.Y;

                float mod = gpadState.Triggers.Right + 1f;
                rocket.ApplyForce(leftStick * mod / 5f);
            }
            if (gpadState.ThumbSticks.Right.LengthSquared > 0.01f)
            {
                Vector2 rightStick = gpadState.ThumbSticks.Right;
                rocket.Rotation = (float)Math.Atan2(rightStick.X, rightStick.Y);
            }

            if (gpadState.Buttons.RightShoulder == ButtonState.Pressed && bulletCounter == 0f)
            {
                firePrimary();
            }
            else if (gpadState.Buttons.LeftShoulder == ButtonState.Pressed && bulletCounter == 0f)
            {
                fireSecondary();
            }

            Window.Title = string.Format("{0} - +{1}",
                ConvertUnits.ToDisplayUnits(rocket.Position),
                ConvertUnits.ToDisplayUnits(rocket.LinearVelocity));
            map.Update(gameTime);

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
            map.Draw(renderer);

            //Draw bullets
            foreach (var bullet in bullets)
                renderer.DrawCircle(ConvertUnits.ToDisplayUnits(bullet.Body.Position), 1f, 4, Color4.Orange);

            //Draw rocket
            renderer.SetTransform(
                Matrix4.CreateRotationZ(rocket.Rotation) *
                Matrix4.CreateTranslation(rocketPosition.X, rocketPosition.Y, 0f));
            renderer.DrawShape(rocketShape.VertexData, ColorUtils.Blend(Color4.LimeGreen, Color4.Red, playerHealth / 100f));
            
            renderer.End();

            interfaceManager.Draw();

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
            rocket.Position = ConvertUnits.ToSimUnits(new Vector2(830f, 298f));
            rocket.AngularDamping = 300f;
            rocket.LinearDamping = 8f;
            rocket.UserData = "Player";
            rocket.OnCollision += Rocket_OnCollision;

            rocketShape = new StaticGeometry(6);
            rocketShape.PhysicsBody = rocket;

            rocketShape.VertexData[0] = 0f;
            rocketShape.VertexData[1] = -height;

            rocketShape.VertexData[2] = -width;
            rocketShape.VertexData[3] = height;

            rocketShape.VertexData[4] = width;
            rocketShape.VertexData[5] = height;
        }
        private void updateUI()
        {
            interfaceManager.SetStats(playerHealth, bulletCount);
            interfaceManager.SetObjectives(map.Enemies.Count, map.Scientists.Count);
        }

        private void firePrimary()
        {
            Vector2 direction = new Vector2(
                (float)Math.Sin(rocket.Rotation),
                -(float)Math.Cos(rocket.Rotation));
            Vector2 velocity = direction / 500f;
            Vector2 position = direction * 6f + ConvertUnits.ToDisplayUnits(rocket.Position);

            spawnBullet(position, velocity, 1f, 1f);

            //Apply inverse force to rocket
            rocket.ApplyForce(-velocity);
        }
        private void fireSecondary()
        {
            Vector2 direction = new Vector2(
                (float)Math.Sin(rocket.Rotation),
                -(float)Math.Cos(rocket.Rotation));

            Vector2 left = new Vector2(
                 (float)Math.Sin(rocket.Rotation - MathHelper.PiOver6),
                -(float)Math.Cos(rocket.Rotation - MathHelper.PiOver6));
            Vector2 right = new Vector2(
                 (float)Math.Sin(rocket.Rotation + MathHelper.PiOver6),
                -(float)Math.Cos(rocket.Rotation + MathHelper.PiOver6));
            
            Vector2 position = direction * 6f + ConvertUnits.ToDisplayUnits(rocket.Position);

            spawnBullet(position, left / 250f, 2f, 2f);
            spawnBullet(position, right / 250f, 2f, 2f);

            //Apply inverse force to rocket
            rocket.ApplyForce(-left / 500f);
            rocket.ApplyForce(-right / 500f);
        }

        private void spawnBullet(Vector2 position, Vector2 velocity, float size, float damage)
        {
            if (bulletCount == 0) return;

            Body physicsBody = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(size), 0.5f);
            physicsBody.Position = ConvertUnits.ToSimUnits(position);
            physicsBody.FixedRotation = true;
            physicsBody.BodyType = BodyType.Dynamic;
            physicsBody.IsBullet = true;
            physicsBody.OnCollision += Bullet_OnCollision;
            physicsBody.ApplyLinearImpulse(velocity);
            physicsBody.CollidesWith = (Category.All ^ Category.Cat1);

            Bullet bullet = new Bullet() { Body = physicsBody, Damage = damage };
            physicsBody.UserData = new object[] { "Player_Bullet", bullet };

            bullets.Add(bullet);
            bulletCount--;

            updateUI();
        }

        private bool Bullet_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.Body.UserData != null && fixtureB.Body.UserData.Equals("Enemy"))
            {
                Bullet bullet = (Bullet)((object[])fixtureA.Body.UserData)[1]; //Gross
                for (int i = 0; i < map.Enemies.Count; i++)
                {
                    if (map.Enemies[i].PhysicsBody.BodyId != fixtureB.Body.BodyId) continue;

                    map.Enemies[i].CurrentHealth -= bullet.Damage;
                    if (map.Enemies[i].CurrentHealth <= 0f)
                    {
                        map.Enemies[i].PhysicsBody.Dispose();
                        map.Enemies.RemoveAt(i--);

                        updateUI();

                        break;
                    }
                }
            }

            for (int i = 0; i < bullets.Count; i++)
            {
                if (bullets[i].Body.BodyId == fixtureA.Body.BodyId)
                {
                    bullets.RemoveAt(i);
                    fixtureA.Body.Dispose();
                }
            }

            return true;
        }
        private bool Rocket_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.Body.UserData == null) return true;

            if (fixtureB.Body.UserData.Equals("Level"))
            {
                float velLength = rocket.LinearVelocity.Length;
                if (velLength > 1f) //Deal damage
                {
                    float damage = 1f * velLength;
                    playerHealth -= damage;

                    if (playerHealth <= 0f)
                        playerHealth = 0f;

                    Window.Title = playerHealth.ToString();
                }
            }

            updateUI();
            return true;
        }
        private void Map_ScientistRetrieved()
        {
            updateUI();
        }
    }

    public class Bullet
    {
        public Body Body;
        public float Lifetime;
        public float Damage;
    }
}
