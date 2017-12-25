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
        private StringRenderer stringRenderer;

        private MainMenu mainMenu;

        public InterfaceManager(ContentManager content)
        {
            fontLibrary = new Library();
            fontFace = new Face(fontLibrary, "Content/Fonts/Jack.ttf");
            fontFace.SetCharSize(0, 18f, 0, 0);

            textureRenderer = new TextureRenderer(content, 800, 450);
            stringRenderer = new StringRenderer();

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
        public StringRenderer StringRenderer { get { return stringRenderer; } }
        public Face DefaultFont { get { return fontFace; } }

        public static bool UpdateUI = false;
    }
}
