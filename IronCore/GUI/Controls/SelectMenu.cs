using System;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;

namespace IronCore.GUI.Controls
{
    public class SelectMenu : Control
    {
        private string[] options;
        private Texture2D renderedText;

        public SelectMenu(params string[] options)
        {
            this.options = options;
        }

        public override void Update(GameTime gameTime)
        {
            
        }
        public override void Draw(GameTime gameTime)
        {
            InterfaceManager.TextureRenderer.Draw(renderedText, Position);
        }
        public override void Load()
        {
            //Dispose of texture 
            if (renderedText != null)
                renderedText.Dispose();

            renderedText = InterfaceManager.StringRenderer.RenderString(
                InterfaceManager.DefaultFont, "TEST MESSAGE", 
                Color4.Black, Color4.White, false);
            area.Size = new Vector2(renderedText.Width, renderedText.Height);
        }
        public override void Unload()
        {
            if (renderedText != null)
            {
                renderedText.Dispose();
                renderedText = null;
            }
        }

        public event Action<string> OptionSelected;
    }
}
