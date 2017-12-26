using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;
using IronCore.GUI.Screens;
using SharpFont;

namespace IronCore
{
    public class InterfaceManager
    {
        private Library fontLibrary;
        private Face fontFace;

        private TextureRenderer textureRenderer;
        private ShapeRenderer shapeRenderer;
        private StringRenderer stringRenderer;
        
        private Dictionary<string, Screen> screens;
        private Screen activeScreen;

        public InterfaceManager(ContentManager content)
        {
            fontLibrary = new Library();
            fontFace = new Face(fontLibrary, "Content/Fonts/Jack.ttf");
            fontFace.SetCharSize(0, 18f, 0, 0);

            textureRenderer = new TextureRenderer(content, 800, 450);
            shapeRenderer = new ShapeRenderer(content, 800, 450);
            stringRenderer = new StringRenderer();

            shapeRenderer.Begin();
            shapeRenderer.SetProjection(Matrix4.CreateOrthographicOffCenter(0f, 800f, 450f, 0f, -1f, 1f));
            shapeRenderer.End();

            screens = new Dictionary<string, Screen>();
            screens.Add("MainMenu", new MainMenu(this));
            screens.Add("GameOverlay", new GameOverlay(this));

            ChangeScreen("GameOverlay");
        }

        public void Update(GameTime gameTime)
        {
            activeScreen.Update(gameTime);

            if (RefreshUI)
            {
                activeScreen.Load();
                RefreshUI = false;
            }
        }
        public void Draw(GameTime gameTime)
        {
            activeScreen.Draw(gameTime);
        }

        public void ChangeScreen(string name)
        {
            if (!screens.ContainsKey(name))
                throw new InvalidOperationException("Invalid screen name.");

            if (activeScreen != null)
                activeScreen.Unload();

            activeScreen = screens[name];
            activeScreen.Load();
        }

        public TextureRenderer TextureRenderer { get { return textureRenderer; } }
        public ShapeRenderer ShapeRenderer { get { return shapeRenderer; } }
        public StringRenderer StringRenderer { get { return stringRenderer; } }
        public Face DefaultFont { get { return fontFace; } }

        public static bool RefreshUI = false;
    }
}
