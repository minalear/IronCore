using System;
using System.Linq;
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
        
        public List<StaticGeometry> StaticGeometry;
        public List<StaticGeometry> WaterBodies;

        public int ScientistCount, EnemyCount;

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
            //Player.Update(gameTime);

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
            //Player.Draw(renderer);

            for (int i = 0; i < StaticGeometry.Count; i++)
            {
                renderer.FillShape(StaticGeometry[i].VertexData, Color4.Black);
                renderer.DrawShape(StaticGeometry[i].VertexData, Color4.LimeGreen);
            }
        }
        public void SpawnBullet(Entity owner, Vector2 position, Vector2 velocity, float size, float damage)
        {
            Body physicsBody = BodyFactory.CreateCircle(World, ConvertUnits.ToSimUnits(size), 0.5f);
            physicsBody.Position = ConvertUnits.ToSimUnits(position);
            physicsBody.FixedRotation = true;
            physicsBody.BodyType = BodyType.Dynamic;
            physicsBody.IsBullet = true;
            physicsBody.OnCollision += Bullet_OnCollision;
            physicsBody.ApplyLinearImpulse(velocity);
            physicsBody.CollidesWith = (Category.All ^ Category.Cat1);

            Bullet bullet = new Bullet(this, owner, damage);
            bullet.SetPhysicsBody(physicsBody);
        }

        private bool Bullet_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.Body.UserData != null && fixtureB.Body.UserData.Equals("Enemy"))
            {
                /*Bullet bullet = (Bullet)((object[])fixtureA.Body.UserData)[1]; //Gross
                for (int i = 0; i < map.Enemies.Count; i++)
                {
                    if (map.Enemies[i].PhysicsBody.BodyId != fixtureB.Body.BodyId) continue;

                    map.Enemies[i].CurrentHealth -= bullet.Damage;
                    if (map.Enemies[i].CurrentHealth <= 0f)
                    {
                        map.Enemies[i].PhysicsBody.Dispose();
                        map.Enemies.RemoveAt(i--);

                        updateUI();

                        break;
                    }
                }*/
            }

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].PhysicsBody.BodyId == fixtureA.Body.BodyId)
                {
                    entities[i].PurgeSelf();
                    break;
                }
            }

            return true;
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
