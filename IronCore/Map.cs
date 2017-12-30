using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using IronCore.Utils;
using IronCore.Entities;

namespace IronCore
{
    public class Map
    {
        public World World;
        public Player Player;
        public Vector2 PlayerStart;
        
        public List<StaticGeometry> StaticGeometry;
        public List<StaticGeometry> WaterBodies;

        public int ScientistCount, EnemyCount;

        private List<Entity> entities;

        public Map()
        {
            World = new World(new Vector2(0f, 9.8f));
            entities = new List<Entity>();
        }
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

            //Draw map entities
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Draw(renderer);
            }

            //TODO: Consider rendering static geometry to a texture for performant rendering
            //Or put all of the static geometry into one VBO and render it in one shot, rather than
            //depend on the shape renderer

            //Draw static geometry
            for (int i = 0; i < StaticGeometry.Count; i++)
            {
                renderer.FillShape(StaticGeometry[i].VertexData, Color4.Black);
                renderer.DrawShape(StaticGeometry[i].VertexData, Color4.LimeGreen);
            }

            //Simulate world
            World.Step(0.01f);
        }
        public void SpawnBullet(Entity owner, Vector2 position, Vector2 velocity, float size, float damage)
        {
            Body physicsBody = BodyFactory.CreateCircle(World, ConvertUnits.ToSimUnits(size), 0.5f);
            physicsBody.Position = ConvertUnits.ToSimUnits(position);
            physicsBody.FixedRotation = true;
            physicsBody.BodyType = BodyType.Dynamic;
            physicsBody.IsBullet = true;
            physicsBody.ApplyLinearImpulse(velocity);
            physicsBody.CollidesWith = (Category.All ^ (Category.Cat1 | Category.Cat3));

            Bullet bullet = new Bullet(this, owner, damage);
            bullet.SetPhysicsBody(physicsBody);

            entities.Add(bullet);
        }
        
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
