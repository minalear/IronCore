using System;
using OpenTK;
using OpenTK.Input;
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

        private World world;
        private Camera camera;
        
        private Map map;

        public MainGame() : base("IronCore - Minalear", 800, 450)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            Window.MouseMove += (sender, e) => InputManager.UpdateMousePosition(e.X, e.Y);
        }

        public override void Initialize()
        {
            world = new World(new Vector2(0f, 9.8f));
            camera = new Camera(Vector2.Zero, 2f);
        }
        public override void LoadContent()
        {
            content = new ContentManager("Content/");
            renderer = new ShapeRenderer(content, Window.Width, Window.Height);

            interfaceManager = new InterfaceManager(content);
            InputManager.Initialize();

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, Window.Height / 2f, 0f));
            
            map = content.LoadMap(world, "Maps/physics_map.json");
            map.Player = new Entities.Player(map);

            updateUI();
        }

        public override void Update(GameTime gameTime)
        {
            InputManager.UpdateInputStates();

            //Update camera
            Vector2 rocketPosition = ConvertUnits.ToDisplayUnits(map.Player.PhysicsBody.Position);
            camera.SetPosition(-rocketPosition);

            Window.Title = string.Format("{0} - +{1}",
                ConvertUnits.ToDisplayUnits(map.Player.PhysicsBody.Position),
                ConvertUnits.ToDisplayUnits(map.Player.PhysicsBody.LinearVelocity));
            map.Update(gameTime);

            //Simulate world
            world.Step(0.01f);

            if (InterfaceManager.UpdateUI)
            {
                updateUI();
                InterfaceManager.UpdateUI = false;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            renderer.Begin();
            renderer.SetCamera(camera.Transform);

            map.Draw(renderer);
            
            renderer.End();

            interfaceManager.Draw();

            Window.SwapBuffers();
        }

        private void updateUI()
        {
            //TODO: Performance improvement here.
            interfaceManager.SetStats(map.Player.Health, map.Player.AmmoCount);
            interfaceManager.SetObjectives(map.EnemyCount, map.ScientistCount);
        }
    }
}
