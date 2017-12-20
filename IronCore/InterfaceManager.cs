using System;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;
using SharpFont;

namespace IronCore
{
    public class InterfaceManager
    {
        private Library fontLibrary;
        private Face fontFace;

        private TextureRenderer textureRenderer;
        private StringRenderer stringRenderer;

        private Texture2D stats, objective;

        public InterfaceManager(ContentManager content)
        {
            fontLibrary = new Library();
            fontFace = new Face(fontLibrary, "Content/Fonts/Jack.ttf");
            fontFace.SetCharSize(0, 18f, 0, 0);

            textureRenderer = new TextureRenderer(content, 800, 450);
            stringRenderer = new StringRenderer();

            stats = stringRenderer.RenderString(fontFace, "FILL", Color4.LimeGreen, Color4.Black, false);
            objective = stringRenderer.RenderString(fontFace, "FILL", Color4.LimeGreen, Color4.Black, false);
        }

        public void Draw()
        {
            textureRenderer.Draw(stats, Vector2.Zero);
            textureRenderer.Draw(objective, new Vector2(800 - objective.Width, 0f));
        }

        public void SetStats(float hp, int ammo)
        {
            stats.Dispose();

            string text = string.Format("LHS Xerxes\n  HP: {0}\nAmmo: {1}", hp, ammo);
            stats = stringRenderer.RenderString(fontFace, text, Color4.LimeGreen, Color4.Black, false);
        }
        public void SetObjectives(int enemies, int scientists)
        {
            objective.Dispose();

            string text = string.Format("Enemies: {0}\nAllies: {1}", enemies, scientists);
            objective = stringRenderer.RenderString(fontFace, text, Color4.LimeGreen, Color4.Black, false);
        }
    }
}
