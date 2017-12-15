using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Newtonsoft.Json;

namespace IronCore.Utils
{
    public sealed class ContentManager
    {
        private string contentDirectory = "/";
        public string ContentDirectory { get { return contentDirectory; } set { contentDirectory = value; } }

        public ContentManager() { }
        public ContentManager(string defaultDirectory)
        {
            contentDirectory = defaultDirectory;
        }

        public Texture2D LoadTexture2D(string path)
        {
            path = checkValidPath(path);

            //Load pixel data from file source using Bitmap from System.Drawing
            var bitmap = new Bitmap(path);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr pixelData = bitmapData.Scan0;

            //Generate texture with the pixel data
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixelData);
            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            //Set wrapping and filter modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            float[] borderColor = new float[] { 1f, 1f, 1f, 0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            //Create our new texture
            Texture2D texture = new Texture2D(textureID, bitmap.Width, bitmap.Height);

            //Cleanup
            bitmap.UnlockBits(bitmapData);
            bitmap.Dispose();

            return texture;
        }
        public ShaderProgram LoadShaderProgram(string vertexPath, string fragmentPath)
        {
            vertexPath = checkValidPath(vertexPath);
            fragmentPath = checkValidPath(fragmentPath);

            //Vertex Shader
            int vertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            string vertexShaderSource = readAllText(vertexPath);
            GL.ShaderSource(vertexShaderID, vertexShaderSource);
            GL.CompileShader(vertexShaderID);
            checkShaderCompilationStatus(vertexShaderID, ShaderType.VertexShader);

            //Fragment Shader
            int fragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);
            string fragmentShaderSource = readAllText(fragmentPath);
            GL.ShaderSource(fragmentShaderID, fragmentShaderSource);
            GL.CompileShader(fragmentShaderID);
            checkShaderCompilationStatus(fragmentShaderID, ShaderType.FragmentShader);

            //Create shader program
            int programID = GL.CreateProgram();

            //Attach shaders to the program and link
            GL.AttachShader(programID, vertexShaderID);
            GL.AttachShader(programID, fragmentShaderID);
            GL.LinkProgram(programID);

            //Detach and clean up the shaders
            GL.DetachShader(programID, vertexShaderID);
            GL.DetachShader(programID, fragmentShaderID);
            GL.DeleteShader(vertexShaderID);
            GL.DeleteShader(fragmentShaderID);

            return new ShaderProgram(programID);
        }
        public ShaderProgram LoadShaderProgram(string vertex, string geometry, string fragment)
        {
            vertex = checkValidPath(vertex);
            geometry = checkValidPath(geometry);
            fragment = checkValidPath(fragment);

            //Vertex Shader
            int vertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            string vertexShaderSource = readAllText(vertex);
            GL.ShaderSource(vertexShaderID, vertexShaderSource);
            GL.CompileShader(vertexShaderID);
            checkShaderCompilationStatus(vertexShaderID, ShaderType.VertexShader);

            //Geometry Shader
            int geometryShaderID = GL.CreateShader(ShaderType.GeometryShader);
            string geometryShaderSource = readAllText(geometry);
            GL.ShaderSource(geometryShaderID, geometryShaderSource);
            GL.CompileShader(geometryShaderID);
            checkShaderCompilationStatus(geometryShaderID, ShaderType.GeometryShader);

            //Fragment Shader
            int fragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);
            string fragmentShaderSource = readAllText(fragment);
            GL.ShaderSource(fragmentShaderID, fragmentShaderSource);
            GL.CompileShader(fragmentShaderID);
            checkShaderCompilationStatus(fragmentShaderID, ShaderType.FragmentShader);

            //Create shader program
            int programID = GL.CreateProgram();

            //Attach shaders to the program and link
            GL.AttachShader(programID, vertexShaderID);
            GL.AttachShader(programID, geometryShaderID);
            GL.AttachShader(programID, fragmentShaderID);
            GL.LinkProgram(programID);

            //Detach and clean up the shaders
            GL.DetachShader(programID, vertexShaderID);
            GL.DetachShader(programID, geometryShaderID);
            GL.DetachShader(programID, fragmentShaderID);
            GL.DeleteShader(vertexShaderID);
            GL.DeleteShader(geometryShaderID);
            GL.DeleteShader(fragmentShaderID);

            return new ShaderProgram(programID);
        }
        public Vertices[] LoadMap(World world, string path)
        {
            path = checkValidPath(path);
            MapFile map = JsonConvert.DeserializeObject<MapFile>(path);

            List<Vertices> mapGeometry = new List<Vertices>();

            int layerIndex = -1;
            for (int i = 0; i < map.Layers.Length; i++)
            {
                if (map.Layers[i].Name == "Collision")
                {
                    layerIndex = i;
                    break;
                }
            }

            //Cannot find collision layer
            if (layerIndex == -1)
                throw new FileLoadException("Cannot find collision layer in map file.");

            for (int i = 0; i < map.Layers[layerIndex].Objects.Length; i++)
            {
                ObjectInfo obj = map.Layers[layerIndex].Objects[i];
                if (obj.Polygon == null) continue;

                Vertices renderGeo = new Vertices(obj.Polygon.Length);
                Vertices logicGeo = new Vertices(obj.Polygon.Length);
                Vector2 objPos = new Vector2(obj.X, obj.Y);

                for (int k = 0; k < obj.Polygon.Length; k++)
                {
                    logicGeo.Add(ConvertUnits.ToSimUnits(obj.Polygon[k]));
                    renderGeo.Add(obj.Polygon[k] + objPos);
                }

                Body body = BodyFactory.CreatePolygon(world, logicGeo, 1f, ConvertUnits.ToSimUnits(objPos));
                mapGeometry.Add(renderGeo);
            }

            return mapGeometry.ToArray();
        }

        private Color4 parseHexString(string str)
        {
            //Remove # prefix
            if (str[0] == '#')
                str = str.Remove(0, 1);

            uint value = uint.Parse(str, System.Globalization.NumberStyles.HexNumber);
            uint r = (value & 0xFF0000) >> 16;
            uint g = (value & 0x00FF00) >> 8;
            uint b = (value & 0x0000FF);

            return new Color4(r, g, b, 255);
        }

        private string readAllText(string path)
        {
            string fileText = string.Empty;
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    fileText = reader.ReadToEnd();
                }
            }

            return fileText;
        }
        private string checkValidPath(string path)
        {
            path = contentDirectory + path;
            if (!File.Exists(path))
                throw new FileNotFoundException(path);
            return path;
        }
        private void checkShaderCompilationStatus(int id, ShaderType type)
        {
            int status = 0;
            GL.GetShader(id, ShaderParameter.CompileStatus, out status);

            if (status == SHADER_COMPILATION_ERROR_CODE)
            {
                string errorMessage = GL.GetShaderInfoLog(id);
                throw new ApplicationException(errorMessage);
            }
        }

        private const int SHADER_COMPILATION_ERROR_CODE = 0;
    }

    public class MapFile
    {
        public string BackgroundColor;
        public int Width, Height;
        public int NextObjectID;
        public LayerInfo[] Layers;
    }
    public class LayerInfo
    {
        public string Name;
        public ObjectInfo[] Objects;
    }
    public class ObjectInfo
    {
        public string Name;
        public int ID;
        public float X, Y;
        public float Width, Height;
        public Vector2[] Polygon;
    }
}
