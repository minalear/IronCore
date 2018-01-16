using System;
using IronCore.Utils;

namespace IronCore
{
    public static class GameManager
    {
        private static MainGame game;
        private static Map activeMap;

        public static void Initialize(MainGame game)
        {
            GameManager.game = game;
        }

        public static void LoadMap(string name)
        {
            activeMap = game.Content.LoadMap($"Maps/{name}.json");
            activeMap.Player = new Entities.Player(activeMap, activeMap.PlayerStart);
        }

        public static void Update(GameTime gameTime)
        {
            activeMap.Update(gameTime);
        }
        public static void Draw(GameTime gameTime, ShapeRenderer renderer)
        {
            activeMap.Draw(renderer);
        }

        public static Map ActiveMap { get { return activeMap; } }
    }
}
