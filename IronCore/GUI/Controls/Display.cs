using System;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;

namespace IronCore.GUI.Controls
{
    public class Display : Control
    {
        private Texture2D texture;
        private string displayText;

        public Display(string text)
        {
            displayText = text;
            TextColor = Color4.LimeGreen;
            BackColor = Color4.Black;
        }
        
        public override void Update(GameTime gameTime) { }
        public override void Draw(GameTime gameTime)
        {
            InterfaceManager.TextureRenderer.Draw(texture, Position);
        }

        public override void Load()
        {
            Unload();

            texture = InterfaceManager.StringRenderer.RenderString(
                InterfaceManager.DefaultFont, displayText,
                TextColor, BackColor, true);
            area.Size = new Vector2(texture.Width, texture.Height);
        }
        public override void Unload()
        {
            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }
        }

        public void SetText(string text)
        {
            displayText = text;
            //Load();
            //TODO: Think of a better way of Loading/Unloading and updating control logic
            //Currently, we're calling SetText from the GameOverlay's Load() function which
            //will cause the Display to double load, but if we call SetText anywhere else,
            //it won't load at all. ???
        }

        public Color4 TextColor { get; set; }
        public Color4 BackColor { get; set; }
        public string Text { get { return displayText; } set { SetText(value); } }
    }
}
