using System;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using IronCore.Utils;

namespace IronCore.Entities
{
    public class Entity
    {
        protected Map map;
        protected Body physicsBody;

        public Entity(Map map)
        {
            this.map = map;
            this.map.Entities.Add(this);
        }

        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(ShapeRenderer renderer) { }

        public virtual void PurgeSelf()
        {
            OnPurge?.Invoke(this);

            DoPurge = true;
            physicsBody.Dispose();
        }
        public virtual void OnEntityCollision(Entity other) { }
        public virtual void OnEntitySeparation(Entity other) { }

        public void SetPhysicsBody(Body body)
        {
            physicsBody = body;
            physicsBody.UserData = this;
            physicsBody.OnCollision += physicsBodyOnCollision;
            physicsBody.OnSeparation += physicsBodyOnSeparation;
        }
        
        protected virtual bool physicsBodyOnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body.UserData != null && fixtureB.Body.UserData.GetType().IsSubclassOf(typeof(Entity)))
            {
                OnEntityCollision((Entity)fixtureB.Body.UserData);
            }

            return true;
        }
        protected virtual void physicsBodyOnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            if (fixtureB.Body.UserData != null && fixtureB.Body.UserData.GetType().IsSubclassOf(typeof(Entity)))
            {
                OnEntitySeparation((Entity)fixtureB.Body.UserData);
            }
        }

        public event Action<Entity> OnPurge;
        
        public Body PhysicsBody { get { return physicsBody; } }
        public bool DoPurge { get; private set; }
        public Map Map { get { return map; } }
    }
}
