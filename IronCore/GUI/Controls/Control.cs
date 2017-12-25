using System;
using OpenTK;
using IronCore.Utils;

namespace IronCore.GUI.Controls
{
    public abstract class Control : IDisposable
    {
        protected RectangleF area;

        public RectangleF Area { get { return area; } set { area = value; } }
        public Vector2 Position { get { return area.Position; } set { area.Position = value; } }

        public Screens.Screen Screen { get; set; }
        public InterfaceManager InterfaceManager { get { return Screen.InterfaceManager; } }

        public Control()
        {
            area = new RectangleF();
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
        public abstract void Load();
        public abstract void Unload();

        public virtual void Dispose()
        {
            Unload();
        }
    }
}
