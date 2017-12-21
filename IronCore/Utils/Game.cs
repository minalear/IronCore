using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace IronCore.Utils
{
    public class Game : IDisposable
    {
        private GameTime gameTime;
        private GameWindow window;

        public Game(string title, int width, int height)
        {
            window = new GameWindow(width, height, new GraphicsMode(32, 24, 8, 4), title, GameWindowFlags.Default);
            //window = new GameWindow(width, height, GraphicsMode.Default, title, GameWindowFlags.Default);
            window.RenderFrame += (sender, e) => renderFrame(e);
            window.UpdateFrame += (sender, e) => updateFrame(e);
            window.Resize += (sender, e) => Resize(Window.Width, Window.Height);

            Console.WriteLine("Vendor:         {0}", GL.GetString(StringName.Vendor));
            Console.WriteLine("Renderer:       {0}", GL.GetString(StringName.Renderer));
            Console.WriteLine("OpenGL Version: {0}", GL.GetString(StringName.Version));
            Console.WriteLine("GLSL Version:   {0}", GL.GetString(StringName.ShadingLanguageVersion));

            gameTime = new GameTime();
            RNG.Init();

            //window.VSync = VSyncMode.Off;

            WINDOW_WIDTH = width;
            WINDOW_HEIGHT = height;

            Initialize();
            LoadContent();
        }

        public virtual void Initialize() { }
        public virtual void LoadContent() { }
        public virtual void Resize(int width, int height)
        {
            WINDOW_WIDTH = width;
            WINDOW_HEIGHT = height;
        }

        public virtual void Draw(GameTime gameTime) { }
        public virtual void Update(GameTime gameTime) { }

        public virtual void Run()
        {
            window.Run(0.0, 0.0);
        }
        public virtual void Stop()
        {
            window.Exit();
        }
        public virtual void Dispose()
        {
            window.Dispose();
        }

        private void renderFrame(FrameEventArgs e)
        {
            Draw(gameTime);
        }
        private void updateFrame(FrameEventArgs e)
        {
            gameTime.ElapsedTime = TimeSpan.FromSeconds(e.Time);
            gameTime.TotalTime.Add(TimeSpan.FromSeconds(e.Time));

            if (Window.Focused || true)
                Update(gameTime);
        }

        //Properties
        public GameWindow Window { get { return window; } }

        public static int WINDOW_WIDTH { get; private set; }
        public static int WINDOW_HEIGHT { get; private set; }
    }
}
