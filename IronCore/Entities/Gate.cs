using System;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics;
using IronCore.Utils;

namespace IronCore.Entities
{
    public class Gate : Entity
    {
        public string Name;
        public bool MoveGate;
        public Vector2 StartPosition, EndPosition;

        public float Timer, OpenTime;

        private RectangleF displayArea;

        public Gate(Map map, RectangleF area) : base(map)
        {
            displayArea = area;
        }

        public override void Update(GameTime gameTime)
        {
            if (!MoveGate) return;

            //Update timer
            Timer += gameTime.FrameDelta;
            if (Timer > OpenTime)
            {
                Timer = OpenTime;
                MoveGate = false;
            }

            //Interpolate the gate's position based on the timer
            PhysicsBody.Position = Vector2.Lerp(StartPosition, EndPosition, Timer / OpenTime);
            displayArea.Position = ConvertUnits.ToDisplayUnits(PhysicsBody.Position) - displayArea.Size / 2f;
        }
        public override void Draw(ShapeRenderer renderer)
        {
            renderer.DrawRect(displayArea, Color4.White);
        }
    }
}
