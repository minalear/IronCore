using System;
using System.Drawing;
using Imaging = System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;

namespace IronCore.Utils
{
    public class Texture2D : IDisposable
    {
        private int textureID;
        private int width, height;

        public Texture2D(int id, int w, int h)
        {
            textureID = id;
            width = w;
            height = h;
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, textureID);
        }
        public void Dispose()
        {
            GL.DeleteTexture(textureID);
        }

        public static Texture2D CreateFromBitmap(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            IntPtr pixelData = bitmapData.Scan0;

            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixelData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //float[] borderColor = new float[] { 0f, 0f, 0f, 0f };
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            bitmap.UnlockBits(bitmapData);

            return new Texture2D(textureID, width, height);
        }

        public int ID { get { return textureID; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
    }
}
