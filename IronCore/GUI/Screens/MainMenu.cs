using System;
using OpenTK;
using IronCore.Utils;
using IronCore.GUI.Controls;

namespace IronCore.GUI.Screens
{
    public class MainMenu : Screen
    {
        public MainMenu(InterfaceManager manager)
            : base(manager)
        {
            SelectMenu menu = new SelectMenu("Option #1", "Option #2");
            menu.Position = new Vector2();

            AddControl(menu);
        }
    }
}
