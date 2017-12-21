using System;
using System.Linq;
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
        public World World;
        public Body PlayerBody;
        
        public List<StaticGeometry> StaticGeometry;
        public List<StaticGeometry> WaterBodies;
        public List<Gate> Gates;
        public List<Sensor> Sensors;
        public List<Enemy> Enemies;
        public List<Scientist> Scientists;

        public Map(World world)
        {
            World = world;
        }

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

            //Update enemy logic
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Update(gameTime, this);
            }

            //Update scientists logic
            for (int i = 0; i < Scientists.Count; i++)
            {
                Scientists[i].Update(gameTime, this);
                if (Scientists[i].DoRemove)
                {
                    Scientists[i].PhysicsBody.Dispose();
                    Scientists.RemoveAt(i--);

                    ScientistRetrieved?.Invoke();
                }
            }
        }
        public void Draw(ShapeRenderer renderer)
        {
            renderer.SetTransform(Matrix4.Identity);

            //Draw water
            for (int i = 0; i < WaterBodies.Count; i++)
            {
                renderer.FillShape(WaterBodies[i].VertexData, new Color4(0, 174, 239, 60));
                renderer.DrawShape(WaterBodies[i].VertexData, new Color4(0, 174, 239, 255));
            }

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
                renderer.DrawCircle(Enemies[i].Area, 24, Enemies[i].Color);
            }
            for (int i = 0; i < Scientists.Count; i++)
            {
                renderer.DrawCircle(Scientists[i].DisplayPosition, 1f, 6, Scientists[i].Color);
            }
        }

        public event Action ScientistRetrieved;
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

        public Color4 Color = Color4.Cyan;

        public void Update(GameTime gameTime, Map map)
        {
            Color = Color4.Cyan;
            float distToPlayer = map.PlayerBody.Position.DistanceSquared(PhysicsBody.Position);
            if (distToPlayer > 50f) return;

            var fixtureList =
                (from fixture in map.World.RayCast(PhysicsBody.Position, map.PlayerBody.Position)
                where fixture.CollisionCategories == Category.Cat2
                select fixture).ToList();
            if (fixtureList.Count <= 1)
                Color = Color4.Red;
        }
    }
    public class Scientist
    {
        public Vector2 DisplayPosition;
        public RectangleF SpawnArea;
        public Body PhysicsBody;
        public Color4 Color = Color4.Aquamarine;

        public bool DoRemove = false;

        private float positionTracker;
        private float positionMod = 1f;

        public Scientist(Vector2 displayPosition, RectangleF spawnArea)
        {
            DisplayPosition = displayPosition;
            SpawnArea = spawnArea;

            //Position between the left and right boundary of the spawn area
            positionTracker = (displayPosition.X - spawnArea.X) / spawnArea.Width;
        }

        public void Update(GameTime gameTime, Map map)
        {
            float distToPlayer = map.PlayerBody.Position.DistanceSquared(PhysicsBody.Position);
            float playerVel = map.PlayerBody.LinearVelocity.LengthSquared;

            if (distToPlayer > 0.1f || playerVel != 0f) //Patrol
            {
                Color = Color4.Aquamarine;

                positionTracker += 0.003f * positionMod;
                if (positionTracker >= 1f)
                {
                    positionTracker = 1f;
                    positionMod = -1f;
                }
                else if (positionTracker <= 0f)
                {
                    positionMod = 0f;
                    positionMod = 1f;
                }
            }
            else if (playerVel == 0f)
            {
                Color = Color4.DarkGoldenrod;

                positionMod = (map.PlayerBody.Position.X < PhysicsBody.Position.X) ? -1f : 1f;
                positionTracker += 0.003f * positionMod;

                if (distToPlayer <= 0.002f)
                {
                    DoRemove = true;
                }
            }

            float y = DisplayPosition.Y;
            DisplayPosition = Vector2.Lerp(new Vector2(SpawnArea.Left, y), new Vector2(SpawnArea.Right, y), positionTracker);
            PhysicsBody.Position = ConvertUnits.ToSimUnits(DisplayPosition);
        }
    }
}
