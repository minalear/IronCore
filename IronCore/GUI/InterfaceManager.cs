using System;
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

        private MainMenu mainMenu;

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

            mainMenu = new MainMenu(this);
            mainMenu.Load();
        }

        public void Update(GameTime gameTime)
        {
            mainMenu.Update(gameTime);

            if (UpdateUI)
            {
                mainMenu.Load();
                UpdateUI = false;
            }
        }
        public void Draw(GameTime gameTime)
        {
            mainMenu.Draw(gameTime);
        }

        public TextureRenderer TextureRenderer { get { return textureRenderer; } }
        public ShapeRenderer ShapeRenderer { get { return shapeRenderer; } }
        public StringRenderer StringRenderer { get { return stringRenderer; } }
        public Face DefaultFont { get { return fontFace; } }

        public static bool UpdateUI = false;
    }
}
