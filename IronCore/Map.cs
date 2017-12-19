using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using IronCore.Utils;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

namespace IronCore
{
    public class Map
    { 
        public List<StaticGeometry> StaticGeometry;
        public List<Gate> Gates;
        public List<Sensor> Sensors;
        public List<Enemy> Enemies;

        public void Update(GameTime gameTime)
        {
            //Update world gates
            for (int i = 0; i < Gates.Count; i++)
            {
                if (!Gates[i].MoveGate) continue;

                //Update timer
                Gates[i].Timer += gameTime.FrameDelta;
                if (Gates[i].Timer > Gates[i].OpenTime)
                {
                    Gates[i].Timer = Gates[i].OpenTime;
                    Gates[i].MoveGate = false;
                }

                //Interpolate Gate's position based on Timer/OpenTime
                Gates[i].PhysicsBody.Position = Vector2.Lerp(
                    Gates[i].StartPosition, Gates[i].EndPosition,
                    Gates[i].Timer / Gates[i].OpenTime);
                Gates[i].DisplayArea.Position = ConvertUnits.ToDisplayUnits(Gates[i].PhysicsBody.Position);
                Gates[i].DisplayArea.Position -= Gates[i].DisplayArea.Size / 2f;
            }
        }
        public void Draw(ShapeRenderer renderer)
        {
            renderer.SetTransform(Matrix4.Identity);

            for (int i = 0; i < Gates.Count; i++)
            {
                renderer.DrawRect(Gates[i].DisplayArea, Color4.White);
            }
            for (int i = 0; i < Sensors.Count; i++)
            {
                Color4 drawColor = Sensors[i].TargetGate.MoveGate ? Color4.MediumPurple : Color4.Orange;
                renderer.DrawRect(Sensors[i].DisplayArea, drawColor);
            }
            for (int i = 0; i < StaticGeometry.Count; i++)
            {
                renderer.FillShape(StaticGeometry[i].VertexData, Color4.Black);
                renderer.DrawShape(StaticGeometry[i].VertexData, Color4.LimeGreen);
            }
            for (int i = 0; i < Enemies.Count; i++)
            {
                renderer.DrawCircle(Enemies[i].Area, 24, Color4.Cyan);
            }
        }
    }

    public class Gate
    {
        public Body PhysicsBody;
        public RectangleF DisplayArea;

        public string Name;
        public bool MoveGate;
        public Vector2 StartPosition, EndPosition;

        public float Timer, OpenTime;
    }
    public class Sensor
    {
        public Body PhysicsBody;

        public string TargetGateName;
        public Gate TargetGate;
        public RectangleF DisplayArea;

        public void OpenGate()
        {
            TargetGate.MoveGate = true;
        }
    }
    public class StaticGeometry
    {
        public Body PhysicsBody;
        public float[] VertexData;

        public StaticGeometry(int vertexCount)
        {
            VertexData = new float[vertexCount];
        }
    }
    public class Enemy
    {
        public CircleF Area;
        public Body PhysicsBody;
        public float CurrentHealth;
    }
}
