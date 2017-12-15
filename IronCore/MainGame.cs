using System;
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

        public MainGame() : base("IronCore - Minalear", 800, 450) { }

        public override void Initialize()
        {
            world = new World(new Vector2(0f, 9.8f));

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

            rocketShape = new Shape(6);

            rocketShape.VertexData[0] = 0f;
            rocketShape.VertexData[1] = -height;

            rocketShape.VertexData[2] = -width;
            rocketShape.VertexData[3] = height;

            rocketShape.VertexData[4] = width;
            rocketShape.VertexData[5] = height;
        }
    }
}
