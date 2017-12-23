using System;
using IronCore.Utils;
using IronCore.Entities;

namespace IronCore.Controllers
{
    public abstract class Controller<T>
    {
        protected T parent;

        public Controller(T entity)
        {
            this.parent = entity;
        }

        public abstract void Update(GameTime gameTime);
        public T Entity { get { return parent; } set { parent = value; } }
    }
}
