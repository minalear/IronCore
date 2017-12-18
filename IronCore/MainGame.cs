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
        private List<Enemy> enemies;
        private float bulletCounter = 0f;
        private float playerHealth = 100f;

        private Gate gate;
        private CircleF sensor;
        private Body sensorBody;

        public MainGame() : base("IronCore - Minalear", 800, 450) { }

        public override void Initialize()
        {
            world = new World(new Vector2(0f, 9.8f));
            bullets = new List<Bullet>();
            enemies = new List<Enemy>();
        }
        public override void LoadContent()
        {
            content = new ContentManager("Content/");
            renderer = new ShapeRenderer(content, Window.Width, Window.Height);

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, Window.Height / 2f, 0f));

            levelGeometry = content.LoadMap(world, "Maps/test_map.json");
            initRocket();
            initEnemies();
            initGates();
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
            
            Vector2 enemyThrust = new Vector2(0f, -9.8f);
            foreach (var enemy in enemies)
                enemy.Body.ApplyForce(enemyThrust);

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
            /*if (gpadState.Buttons.A == ButtonState.Pressed)
            {
                float x = (float)Math.Sin(rocket.Rotation);
                float y = -(float)Math.Cos(rocket.Rotation);

                rocket.ApplyForce(new Vector2(0f, y) / 5f);
            }*/

            if (gpadState.Buttons.RightShoulder == ButtonState.Pressed && bulletCounter == 0f)
            {
                spawnBullet();
            }

            Window.Title = ConvertUnits.ToDisplayUnits(rocket.Position).ToString();
            //Window.Title = ConvertUnits.ToDisplayUnits(rocket.LinearVelocity).ToString();

            if (gate.OpenGate)
            {
                gate.GateTimer += gameTime.FrameDelta;
                if (gate.GateTimer >= 5f)
                {
                    gate.GateTimer = 5f;
                    gate.OpenGate = false;
                }

                gate.Body.Position = Vector2.Lerp(gate.ClosedPosition, gate.OpenPosition, gate.GateTimer / 5f);
                gate.Shape.Position = ConvertUnits.ToDisplayUnits(gate.Body.Position) - gate.Shape.Size / 2f;
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
            renderer.DrawRect(gate.Shape, Color4.White);
            renderer.DrawCircle(sensor, 32, gate.SensorColor);
            foreach (Shape shape in levelGeometry)
            {
                renderer.FillShape(shape.VertexData, Color4.Black);
                renderer.DrawShape(shape.VertexData, Color4.LimeGreen);
            }

            //Draw bullets
            foreach (var bullet in bullets)
                renderer.DrawCircle(ConvertUnits.ToDisplayUnits(bullet.Body.Position), 1f, 4, Color4.Orange);

            //Draw enemies
            foreach (var enemy in enemies)
                renderer.DrawCircle(ConvertUnits.ToDisplayUnits(enemy.Body.Position), 6f, 24, Color4.Cyan);

            //Draw rocket
            renderer.SetTransform(
                Matrix4.CreateRotationZ(rocket.Rotation) *
                Matrix4.CreateTranslation(rocketPosition.X, rocketPosition.Y, 0f));
            renderer.DrawShape(rocketShape.VertexData, ColorUtils.Blend(Color4.LimeGreen, Color4.Red, playerHealth / 100f));
            
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
            rocket.OnCollision += Rocket_OnCollision;

            rocketShape = new Shape(6);

            rocketShape.VertexData[0] = 0f;
            rocketShape.VertexData[1] = -height;

            rocketShape.VertexData[2] = -width;
            rocketShape.VertexData[3] = height;

            rocketShape.VertexData[4] = width;
            rocketShape.VertexData[5] = height;
        }
        private void initEnemies()
        {
            var enemyPositions = new Vector2[]
            {
                new Vector2(35f, 50f),
                new Vector2(128f, 430f),
                new Vector2(180f, 560f),
                new Vector2(900f, 260f)
            };

            foreach (var position in enemyPositions)
            {
                Body enemy = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(6f), 88.5f, ConvertUnits.ToSimUnits(position));
                enemy.CollisionCategories = Category.Cat2;
                enemy.BodyType = BodyType.Dynamic;
                enemy.UserData = "Enemy";

                enemies.Add(new Enemy() { Body = enemy, Health = 10f });
            }
        }
        private void initGates()
        {
            var shape = new RectangleF(60f, 125f, 100f, 10f);
            Vector2 size = ConvertUnits.ToSimUnits(shape.Size);
            Vector2 pos = ConvertUnits.ToSimUnits(shape.Position) + size / 2;

            gate = new Gate()
            {
                Body = BodyFactory.CreateRectangle(world, size.X, size.Y, 1f, pos, 0f, BodyType.Static, "Gate"),
                Shape = shape,
                OpenPosition = ConvertUnits.ToSimUnits(new Vector2(250f, 125f))
            };
            gate.Body.CollisionCategories = Category.Cat2;
            gate.ClosedPosition = gate.Body.Position;

            sensor = new CircleF(200f, 100f, 12f);
            sensorBody = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(sensor.Radius), 1f, ConvertUnits.ToSimUnits(sensor.Position), BodyType.Static, "Sensor");
            sensorBody.IsSensor = true;

            sensorBody.OnCollision += SensorBody_OnCollision;
            sensorBody.OnSeparation += SensorBody_OnSeparation;
        }
        
        private void spawnBullet()
        {
            Vector2 direction = new Vector2(
                (float)Math.Sin(rocket.Rotation),
                -(float)Math.Cos(rocket.Rotation));
            Vector2 velocity = direction / 500f;
            Vector2 position = direction * 6f + ConvertUnits.ToDisplayUnits(rocket.Position);

            Body bullet = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(1f), 0.5f);
            bullet.Position = ConvertUnits.ToSimUnits(position);
            bullet.FixedRotation = true;
            bullet.BodyType = BodyType.Dynamic;
            bullet.IsBullet = true;
            bullet.OnCollision += Bullet_OnCollision;
            bullet.ApplyLinearImpulse(velocity);
            bullet.CollidesWith = (Category.All ^ Category.Cat1);

            bullet.UserData = "Player_Bullet";

            bullets.Add(new Bullet() { Body = bullet });

            //Apply inverse force to rocket
            rocket.ApplyForce(-velocity);
        }

        private bool Bullet_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.Body.UserData != null && fixtureB.Body.UserData.Equals("Enemy"))
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Body.BodyId != fixtureB.Body.BodyId) continue;

                    enemies[i].Health -= 1f;
                    if (enemies[i].Health <= 0f)
                    {
                        enemies[i].Body.Dispose();
                        enemies.RemoveAt(i--);

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
            if (fixtureB.Body.UserData != null && fixtureB.Body.UserData.Equals("Level"))
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

            return true;
        }
        private bool SensorBody_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.Body.UserData.Equals("Player"))
            {
                gate.SensorColor = Color4.MediumPurple;
                gate.OpenGate = true;

                return true;
            }
            
            return false;
        }
        private void SensorBody_OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            gate.SensorColor = Color4.Orange;
        }
    }

    public class Bullet
    {
        public Body Body;
        public float Lifetime;
    }
    public class Enemy
    {
        public Body Body;
        public float Health;
    }
    public class Gate
    {
        public Body Body;
        public RectangleF Shape;
        public bool OpenGate = false;
        public Vector2 OpenPosition, ClosedPosition;
        public float GateTimer = 0f;

        public Color4 SensorColor = Color4.Orange;
    }
}
