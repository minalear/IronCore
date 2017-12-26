using System;
using OpenTK;
using IronCore.Utils;
using IronCore.GUI.Controls;

namespace IronCore.GUI.Screens
{
    public class GameOverlay : Screen
    {
        private Display stats, objective;

        public GameOverlay(InterfaceManager interfaceManager)
            : base(interfaceManager)
        {
            stats = new Display("NULL");
            stats.Position = Vector2.Zero;

            objective = new Display("NULL");
            objective.Position = new Vector2(800f - 125f, 0f);

            AddControl(stats);
            AddControl(objective);
        }

        public override void Load()
        {
            stats.SetText("LHS Xavier");
            objective.SetText("Stats Here.");

            base.Load();
        }
    }
}
