using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace IronCore.Utils
{
    public class TextureRenderer : IDisposable
    {
        private ShaderProgram shader;
        private VertexArray vertexArray;

        public TextureRenderer(ContentManager content, int width, int height)
        {
            shader = content.LoadShaderProgram("Shaders/texture.vs", "Shaders/texture.fs");
            shader.Use();
            shader.SetMatrix4("model", false, Matrix4.Identity);
            shader.SetMatrix4("proj", false, Matrix4.CreateOrthographicOffCenter(0f, width, height, 0f, -1f, 1f));
            shader.SetMatrix4("view", false, Matrix4.Identity);
            shader.SetInt("texture", 0);
            shader.Clear();

            float[] vertexData = new float[] {
                0f, 0f,     0f, 0f,
                0f, 1f,     0f, 1f,
                1f, 0f,     1f, 0f,

                1f, 0f,     1f, 0f,
                0f, 1f,     0f, 1f,
                1f, 1f,     1f, 1f,
            };

            vertexArray = new VertexArray();
            vertexArray.Bind();
            vertexArray.SetBufferData(vertexData);
            vertexArray.EnableAttribute(0, 2, VertexAttribPointerType.Float, 4, 0);
            vertexArray.EnableAttribute(1, 2, VertexAttribPointerType.Float, 4, 2);
            vertexArray.Unbind();
        }

        public void Resize(int width, int height)
        {
            shader.SetMatrix4("proj", false, Matrix4.CreateOrthographicOffCenter(0f, width, height, 0f, -1f, 1f));
        }

        public void Draw(Texture2D texture, Vector2 position)
        {
            shader.Use();
            shader.SetMatrix4("model", false, 
                Matrix4.CreateScale(texture.Width, texture.Height, 1f) * 
                Matrix4.CreateTranslation(position.X, position.Y, 0f));
            shader.SetVector4("source", new Vector4(0f, 0f, 1f, 1f));

            renderTexture(texture);
        }
        public void Draw(Texture2D texture, Vector2 position, Vector2 size)
        {
            shader.Use();
            shader.SetMatrix4("model", false,
                Matrix4.CreateScale(size.X, size.Y, 1f) *
                Matrix4.CreateTranslation(position.X, position.Y, 0f));
            shader.SetVector4("source", new Vector4(0f, 0f, 1f, 1f));

            renderTexture(texture);
        }
        public void Draw(Texture2D texture, Vector2 position, Vector2 size, RectangleF source)
        {
            shader.Use();
            shader.SetMatrix4("model", false,
                Matrix4.CreateScale(size.X, size.Y, 1f) *
                Matrix4.CreateTranslation(position.X, position.Y, 0f));

            source.X /= texture.Width;
            source.Y /= texture.Height;

            source.Width /= texture.Width;
            source.Height /= texture.Height;

            shader.SetVector4("source", source.Vec4);

            renderTexture(texture);
        }

        private void renderTexture(Texture2D texture)
        {
            vertexArray.Bind();

            GL.ActiveTexture(TextureUnit.Texture0);
            texture.Bind();

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            vertexArray.Unbind();
            shader.Clear();
        }

        public void Dispose()
        {
            shader.Dispose();
            vertexArray.Dispose();
        }
    }
}
