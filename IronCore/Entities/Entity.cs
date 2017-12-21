using System;
using OpenTK;
using OpenTK.Graphics;
using FarseerPhysics.Dynamics;
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

        public virtual void Purge()
        {
            DoPurge = true;
            physicsBody.Dispose();
        }
        
        public Body PhysicsBody { get { return physicsBody; } set { physicsBody = value; } }
        public bool DoPurge { get; protected set; }
    }
}
