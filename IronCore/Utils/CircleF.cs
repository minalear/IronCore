using System;
using OpenTK;

namespace IronCore.Utils
{
    public struct CircleF
    {
        private Vector2 position;
        private float radius;

        public CircleF(float x, float y, float radius)
        {
            position = new Vector2(x, y);
            this.radius = radius;
        }

        public bool Contains(Vector2 point)
        {
            return Contains(point.X, point.Y);
        }
        public bool Contains(float x, float y)
        {
            //Distance formula without the square root to increase calculation speed
            float distSqr = (float)(Math.Pow(X - x, 2.0) + Math.Pow(Y - y, 2.0));
            return distSqr <= (radius * radius);
        }

        public void Inflate(float amount)
        {
            radius += amount;
        }

        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }
        public Vector2 Position { get { return position; } set { position = value; } }
        public float Radius { get { return radius; } set { radius = value; } }

        public float Diameter { get { return radius * 2; } }
        public float Circumference { get { return 2 * MathHelper.Pi * radius; } }
    }
}
