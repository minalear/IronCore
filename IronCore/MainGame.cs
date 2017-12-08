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

        }

        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Window.SwapBuffers();
        }
    }
}
