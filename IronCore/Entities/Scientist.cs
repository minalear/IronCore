using System;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;
using IronCore.Controllers;

namespace IronCore.Entities
{
    public class Scientist : Entity
    {
        public Vector2 DisplayPosition;
        public RectangleF SpawnArea;
        public Color4 Color = Color4.Aquamarine;

        private ScientistController controller;

        public Scientist(Map map, Vector2 displayPosition, RectangleF spawnArea) : base(map)
        {
            DisplayPosition = displayPosition;
            SpawnArea = spawnArea;

            controller = new ScientistController(this);
        }

        public override void Update(GameTime gameTime)
        {
            controller.Update(gameTime);
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawCircle(DisplayPosition, 1f, 6, Color);
        }
    }
}
