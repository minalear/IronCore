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
            SetOptions(options);
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.IsButtonReleased(OpenTK.Input.Buttons.A))
            {
                SetOptions("WOH", "WHAT", "THE", "FUCK");
                InterfaceManager.UpdateUI = true;
            }
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

            //Create a single string of all the options
            string formattedString = string.Empty;
            for (int i = 0; i < options.Length; i++)
                formattedString += options[i] + "\n";

            //Render the string to a texture and update the control size
            renderedText = InterfaceManager.StringRenderer.RenderString(
                InterfaceManager.DefaultFont, formattedString, 
                Color4.Black, Color4.White, true);
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

        public void SetOptions(params string[] options)
        {
            this.options = options;
        }

        public event Action<string> OptionSelected;
    }
}
