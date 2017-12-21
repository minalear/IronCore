using System;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics.Dynamics;
using IronCore.Utils;

namespace IronCore.Entities
{
    public class Sensor : Entity
    {
        public string TargetGateName;
        public Gate TargetGate;
        public RectangleF DisplayArea;

        public Sensor(Map map) : base(map) { }

        public void OpenGate()
        {
            TargetGate.MoveGate = true;
        }

        public override void Draw(ShapeRenderer renderer)
        {
            Color4 drawColor = TargetGate.MoveGate ? Color4.MediumPurple : Color4.Orange;
            renderer.DrawRect(DisplayArea, drawColor);
        }

        public override void OnEntityCollision(Entity other)
        {
            if (other.GetType() == typeof(Player))
            {
                OpenGate();
            }
        }
    }
}
