﻿using OpenTK;

namespace IronCore.Utils
{
    public struct RectangleF
    {
        private float x, y, width, height;

        public RectangleF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        public RectangleF(Vector2 position, Vector2 size)
        {
            x = position.X;
            y = position.Y;
            width = size.X;
            height = size.Y;
        }

        public bool Contains(float x, float y)
        {
            return (x >= X && x <= Right && y >= Y && y <= Bottom);
        }
        public bool Contains(Vector2 position)
        {
            return (position.X >= X && position.X <= Right && position.Y >= Y && position.Y <= Bottom);
        }
        public bool Contains(RectangleF rectangle)
        {
            return (rectangle.Left > Left &&
                    rectangle.Right < Right &&
                    rectangle.Top > Top &&
                    rectangle.Bottom < Bottom);
        }
        public void Inflate(float width, float height)
        {
            float xOffset = width / 2;
            float yOffset = height / 2;

            Left += xOffset;
            Right += xOffset;
            Top += yOffset;
            Bottom += yOffset;
        }

        public Vector2 Position { get { return new Vector2(x, y); } set { x = value.X; y = value.Y; } }
        public Vector2 Size { get { return new Vector2(width, height); } set { width = value.X; height = value.Y; } }
        public Vector4 Vec4 { get { return new Vector4(x, y, width, height); } }
        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }
        public float Width { get { return width; } set { width = value; } }
        public float Height { get { return height; } set { height = value; } }
        public float Left { get { return x; } set { x = value; } }
        public float Top { get { return y; } set { y = value; } }
        public float Right { get { return x + width; } set { width = value - x; } }
        public float Bottom { get { return y + height; } set { height = value - y; } }

        public static RectangleF Empty = new RectangleF();
    }
}
