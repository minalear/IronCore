using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using IronCore.Utils;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

namespace IronCore
{
    public class MainGame : Game
    {
        private ContentManager content;
        private ShapeRenderer renderer;

        private InterfaceManager interfaceManager;

        private Camera camera;

        public MainGame() : base("IronCore - Minalear", 800, 450)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            Window.MouseMove += (sender, e) => InputManager.UpdateMousePosition(e.X, e.Y);
        }

        public override void Initialize()
        {
            camera = new Camera(Vector2.Zero, 2f);
        }
        public override void LoadContent()
        {
            content = new ContentManager("Content/");
            renderer = new ShapeRenderer(content, Window.Width, Window.Height);
            
            InputManager.Initialize();
            GameManager.Initialize(this);
            GameManager.LoadMap("level_01");
            interfaceManager = new InterfaceManager(content);

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, Window.Height / 2f, 0f));
        }

        public override void Update(GameTime gameTime)
        {
            InputManager.UpdateInputStates();
            interfaceManager.Update(gameTime);

            //Update camera
            Vector2 rocketPosition = ConvertUnits.ToDisplayUnits(GameManager.ActiveMap.Player.PhysicsBody.Position);
            camera.SetPosition(-rocketPosition);
            
            GameManager.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            renderer.Begin();
            renderer.SetCamera(camera.Transform);

            GameManager.Draw(gameTime, renderer);
            
            renderer.End();

            interfaceManager.Draw(gameTime);

            Window.SwapBuffers();
        }

        public ContentManager Content { get { return content; } }
    }
}
