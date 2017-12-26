using System;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;

namespace IronCore.GUI.Controls
{
    public class SelectMenu : Control
    {
        private string[] options;
        private Texture2D[] textures;
        private int selectedIndex = 0;

        public SelectMenu(params string[] options)
        {
            SetOptions(options);
        }

        public override void Update(GameTime gameTime)
        {
            //TODO: Replace these with event registers
            if (InputManager.IsButtonReleased(OpenTK.Input.Buttons.A))
            {
                OptionSelected?.Invoke(options[selectedIndex]);
                selectedIndex = 0;
                InterfaceManager.UpdateUI = true;
            }
            if (InputManager.IsDpadButtonReleased(OpenTK.Input.Buttons.DPadDown))
            {
                selectedIndex++;
                if (selectedIndex == options.Length)
                    selectedIndex = 0;
                Load();
            }
            else if (InputManager.IsDpadButtonReleased(OpenTK.Input.Buttons.DPadUp))
            {
                selectedIndex--;
                if (selectedIndex == -1)
                    selectedIndex = options.Length - 1;
                Load();
            }
        }
        public override void Draw(GameTime gameTime)
        {
            //Draw emtpy rectangle
            InterfaceManager.ShapeRenderer.Begin();
            InterfaceManager.ShapeRenderer.FillRect(area, Color4.White);
            InterfaceManager.ShapeRenderer.End();

            float y = Position.Y;
            for (int i = 0; i < textures.Length; i++)
            {
                InterfaceManager.TextureRenderer.Draw(textures[i], new Vector2(Position.X, y));
                y += textures[i].Height;
            }
        }
        public override void Load()
        {
            //Dispose of texture 
            Unload(); //Shortcut disposing all textures
            textures = new Texture2D[options.Length];
            area.Size = Vector2.Zero; //Reset size

            //Render each option to its own texture
            for (int i = 0; i < options.Length; i++)
            {
                Color4 foreColor = (i == selectedIndex) ? Color4.Red : Color4.Black;

                textures[i] = InterfaceManager.StringRenderer.RenderString(
                    InterfaceManager.DefaultFont, options[i],
                    foreColor, Color4.White, false);

                //Modify control size with texture sizes
                area.Height += textures[i].Height;
                area.Width = (textures[i].Width > area.Width) ? textures[i].Width : area.Width;
            }
        }
        public override void Unload()
        {
            //Cleanup textures
            if (textures == null) return;
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null) continue;

                textures[i].Dispose();
                textures[i] = null;
            }
            textures = null;
        }

        public void SetOptions(params string[] options)
        {
            this.options = options;
        }

        public event Action<string> OptionSelected;
    }
}
