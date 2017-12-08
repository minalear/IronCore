using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using IronCore.Utils;
using IronCore.Physics;

namespace IronCore
{
    public class MainGame : Game
    {
        private ContentManager content;
        private ShapeRenderer renderer;

        private World world;

        public MainGame() : base("IronCore - Minalear", 1280, 720) { }

        public override void Initialize()
        {
            world = new World();
        }
        public override void LoadContent()
        {
            content = new ContentManager("Content/");
            renderer = new ShapeRenderer(content, Window.Width, Window.Height);

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, 0f, 0f));
        }

        public override void Update(GameTime gameTime)
        {
            world.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            world.Draw(renderer);

            Window.SwapBuffers();
        }
    }
}
