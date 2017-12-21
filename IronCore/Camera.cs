using OpenTK;

namespace IronCore
{
    public class Camera
    {
        private Matrix4 transform;
        private Vector2 position;
        private float scale;

        public Camera(Vector2 position, float scale)
        {
            SetPositionAndScale(position, scale);
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
            updateTransform();
        }
        public void SetScale(float scale)
        {
            this.scale = scale;
            updateTransform();
        }
        public void SetPositionAndScale(Vector2 position, float scale)
        {
            this.position = position;
            this.scale = scale;
            updateTransform();
        }

        private void updateTransform()
        {
            transform =
                Matrix4.CreateTranslation(position.X, position.Y, 0f) *
                Matrix4.CreateScale(scale);
        }

        public Matrix4 Transform { get { return transform; } }
        public Vector2 Position { get { return position; } set { SetPosition(value); } }
        public float Scale { get { return scale; } set { SetScale(value); } }
    }
}
