using System;
using System.Linq;
using System.Collections.Generic;
using IronCore.Utils;
using IronCore.GUI.Controls;

namespace IronCore.GUI.Screens
{
    public class Screen : IDisposable
    {
        private InterfaceManager interfaceManager;
        protected List<Control> controls;

        public Screen(InterfaceManager interfaceManager)
        {
            this.interfaceManager = interfaceManager;
            controls = new List<Control>();
        }

        public void AddControl(Control control)
        {
            control.Screen = this;
            controls.Add(control);
        }

        public virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                controls[i].Update(gameTime);
            }
        }
        public virtual void Draw(GameTime gameTime)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                controls[i].Draw(gameTime);
            }
        }
        public virtual void Load()
        {
            for (int i = 0; i < controls.Count; i++)
            {
                controls[i].Load();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < controls.Count; i++)
                controls[i].Dispose();
        }

        public InterfaceManager InterfaceManager { get { return interfaceManager; } }
    }
}
