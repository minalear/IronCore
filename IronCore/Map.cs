using System;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using IronCore.Utils;
using IronCore.Entities;

namespace IronCore
{
    public class Map
    {
        public World World;
        public Body PlayerBody;
        
        public List<StaticGeometry> StaticGeometry;
        public List<StaticGeometry> WaterBodies;

        private List<Entity> entities;

        public Map(World world)
        {
            World = world;
            entities = new List<Entity>();
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Update(gameTime);

                if (entities[i].DoPurge)
                {
                    entities.RemoveAt(i--);
                }
            }

            //Update world gates
            /*

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
            }*/
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

            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Draw(renderer);
            }

            for (int i = 0; i < StaticGeometry.Count; i++)
            {
                renderer.FillShape(StaticGeometry[i].VertexData, Color4.Black);
                renderer.DrawShape(StaticGeometry[i].VertexData, Color4.LimeGreen);
            }
        }

        public event Action ScientistRetrieved;
        public List<Entity> Entities { get { return entities; } }
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
}
