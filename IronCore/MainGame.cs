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

        private Body rocket;

        public MainGame() : base("IronCore - Minalear", 800, 450) { }

        public override void Initialize()
        {

        }
        public override void LoadContent()
        {
            content = new ContentManager("Content/");
            renderer = new ShapeRenderer(content, Window.Width, Window.Height);

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, Window.Height / 2f, 0f));
        }

        public override void Update(GameTime gameTime)
        {
            
        }
        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);



            Window.SwapBuffers();
        }
    }
}
