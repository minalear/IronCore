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
            SelectMenu menu = new SelectMenu("New Game", "Load Game", "Options", "Exit");
            menu.Position = new Vector2();
            menu.OptionSelected += (option) =>
            {
                if (option.Equals("New Game", StringComparison.Ordinal))
                    InterfaceManager.ChangeScreen("GameOverlay");
            };

            AddControl(menu);
        }
    }
}
