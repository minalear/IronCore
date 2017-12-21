﻿using System;
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
        private Camera camera;
        
        private Map map;

        public MainGame() : base("IronCore - Minalear", 800, 450)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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

            renderer.SetCamera(Matrix4.CreateTranslation(Window.Width / 2f, Window.Height / 2f, 0f));
            
            map = content.LoadMap(world, "Maps/physics_map.json");
            map.Player = new Entities.Player(map);

            map.ScientistRetrieved += Map_ScientistRetrieved;

            updateUI();
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState gpadState = GamePad.GetState(0);

            //Update camera
            Vector2 rocketPosition = ConvertUnits.ToDisplayUnits(map.Player.PhysicsBody.Position);
            camera.SetPosition(-rocketPosition);

            Window.Title = string.Format("{0} - +{1}",
                ConvertUnits.ToDisplayUnits(map.Player.PhysicsBody.Position),
                ConvertUnits.ToDisplayUnits(map.Player.PhysicsBody.LinearVelocity));
            map.Update(gameTime);

            //Simulate world
            world.Step(0.01f);
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
            //interfaceManager.SetStats(playerHealth, bulletCount);
            //interfaceManager.SetObjectives(map.Enemies.Count, map.Scientists.Count);
        }

        private void Map_ScientistRetrieved()
        {
            updateUI();
        }
    }
}
