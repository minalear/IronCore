using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using IronCore.Utils;

namespace IronCore
{
    public class MainGame : Game
    {
        public MainGame() : base("IronCore - Minalear", 1280, 720)
        {
            for (int i = 0; i < 100; i++)
            {
                Vector2 a = Vector2.Zero;
                Vector2 b = RNG.NextVector();

                Console.WriteLine("{0} => {1} = {2}", a, b, a.Distance(b));
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Window.SwapBuffers();
        }
    }
}
