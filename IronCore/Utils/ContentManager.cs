using System;
using System.Linq;
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
using IronCore.Entities;

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
        public Map LoadMap(World world, string path)
        {
            path = checkValidPath(path);
            MapFile mapData = JsonConvert.DeserializeObject<MapFile>(readAllText(path));

            //TODO: Refactor this garbage
            int collisionIndex = -1;
            int gateIndex = -1;
            int enemyIndex = -1;
            int objectiveIndex = -1;
            int waterIndex = -1; 

            for (int i = 0; i < mapData.Layers.Length; i++)
            {
                if (mapData.Layers[i].Name.Equals("Collision", StringComparison.OrdinalIgnoreCase) &&
                    mapData.Layers[i].Type.Equals("objectgroup", StringComparison.OrdinalIgnoreCase))
                {
                    collisionIndex = i;
                }
                if (mapData.Layers[i].Name.Equals("Gates", StringComparison.OrdinalIgnoreCase) &&
                    mapData.Layers[i].Type.Equals("objectgroup", StringComparison.OrdinalIgnoreCase))
                {
                    gateIndex = i;
                }
                if (mapData.Layers[i].Name.Equals("Enemies", StringComparison.OrdinalIgnoreCase) &&
                    mapData.Layers[i].Type.Equals("objectgroup", StringComparison.OrdinalIgnoreCase))
                {
                    enemyIndex = i;
                }
                if (mapData.Layers[i].Name.Equals("Objectives", StringComparison.OrdinalIgnoreCase) &&
                    mapData.Layers[i].Type.Equals("objectgroup", StringComparison.OrdinalIgnoreCase))
                {
                    objectiveIndex = i;
                }
                if (mapData.Layers[i].Name.Equals("Water", StringComparison.OrdinalIgnoreCase) &&
                    mapData.Layers[i].Type.Equals("objectgroup", StringComparison.OrdinalIgnoreCase))
                {
                    waterIndex = i;
                }
            }

            //TODO: Organize and figure out collision categories

            //Check for valid layers
            if (collisionIndex == -1)
                throw new FileLoadException("Could not find collision layer in map file.");
            if (gateIndex == -1)
                throw new FileLoadException("Could not find gate layer in map file.");
            if (enemyIndex == -1)
                throw new FileLoadException("Could not find enemies layer in map file.");
            if (objectiveIndex == -1)
                throw new FileLoadException("Could not find objective layer in map file.");

            Map map = new Map(world);

            var staticLevelGeometry = new List<StaticGeometry>();

            //Load static geometry
            for (int i = 0; i < mapData.Layers[collisionIndex].Objects.Length; i++)
            {
                ObjectInfo collisionObject = mapData.Layers[collisionIndex].Objects[i];
                if (collisionObject.Polygon == null) continue;

                Vertices simGeo = new Vertices(collisionObject.Polygon.Length);
                StaticGeometry disGeo = new StaticGeometry(collisionObject.Polygon.Length * 2);

                //Loop through the object's vertices (Vector2)
                for (int k = 0; k < collisionObject.Polygon.Length; k++)
                {
                    simGeo.Add(ConvertUnits.ToSimUnits(collisionObject.Polygon[k]));

                    disGeo.VertexData[k * 2 + 0] = collisionObject.Polygon[k].X + collisionObject.X;
                    disGeo.VertexData[k * 2 + 1] = collisionObject.Polygon[k].Y + collisionObject.Y;
                }

                Body geoBody = BodyFactory.CreatePolygon(world, simGeo, 1f);
                geoBody.Position = ConvertUnits.ToSimUnits(new Vector2(collisionObject.X, collisionObject.Y));
                geoBody.UserData = "Static Geometry";
                geoBody.CollisionCategories = Category.Cat2;

                disGeo.PhysicsBody = geoBody;
                staticLevelGeometry.Add(disGeo);
            }

            //Load Gates and Sensors
            for (int i = 0; i < mapData.Layers[gateIndex].Objects.Length; i++)
            {
                ObjectInfo mapObject = mapData.Layers[gateIndex].Objects[i];

                RectangleF area = new RectangleF(mapObject.X, mapObject.Y, mapObject.Width, mapObject.Height);
                Vector2 simPosition = ConvertUnits.ToSimUnits(area.Position + new Vector2(area.Width, area.Height) / 2f);
                Vector2 simSize = ConvertUnits.ToSimUnits(area.Size);

                Body physicsBody = BodyFactory.CreateRectangle(world, simSize.X, simSize.Y, 1f, simPosition);
                physicsBody.BodyType = BodyType.Static;

                if (mapObject.Type.Equals("Gate"))
                {
                    physicsBody.CollisionCategories = Category.Cat2;

                    Gate gate = new Gate(map, area);
                    gate.Name = mapObject.Name;
                    gate.StartPosition = simPosition;
                    gate.SetPhysicsBody(physicsBody);

                    //Gate end positions based on how it slides
                    if (mapObject.Properties["Slide"].Equals("Left"))
                        gate.EndPosition = simPosition - new Vector2(simSize.X, 0f);
                    else if (mapObject.Properties["Slide"].Equals("Right"))
                        gate.EndPosition = simPosition + new Vector2(simSize.X, 0f);
                    else if (mapObject.Properties["Slide"].Equals("Up"))
                        gate.EndPosition = simPosition - new Vector2(0f, simSize.Y);
                    else if (mapObject.Properties["Slide"].Equals("Down"))
                        gate.EndPosition = simPosition + new Vector2(0f, simSize.Y);

                    gate.Timer = 0f;
                    gate.OpenTime = mapObject.Properties.ContainsKey("OpenTime") ? Convert.ToSingle(mapObject.Properties["OpenTime"]) : 5f;
                }
                else if (mapObject.Type.Equals("Sensor"))
                {
                    physicsBody.IsSensor = true;
                    physicsBody.CollisionCategories = Category.Cat1;

                    Sensor sensor = new Sensor(map);
                    sensor.SetPhysicsBody(physicsBody);
                    sensor.TargetGateName = mapObject.Properties.ContainsKey("Gate") ? (string)mapObject.Properties["Gate"] : "None";
                    sensor.DisplayArea = area;
                }
            }

            //Pair sensors with gates
            var gates = (from entity in map.Entities
                         where entity.GetType() == typeof(Gate)
                         select (Gate)entity).ToList();
            var sensors = (from entity in map.Entities
                         where entity.GetType() == typeof(Sensor)
                         select (Sensor)entity).ToList();

            for (int i = 0; i < sensors.Count; i++)
            {
                if (sensors[i].TargetGateName.Equals("None")) continue;

                for (int k = 0; k < gates.Count; k++)
                {
                    if (gates[k].Name == sensors[i].TargetGateName)
                    {
                        //Set target gate and setup event trigger
                        Sensor sensor = sensors[i];
                        sensor.TargetGate = gates[k];
                        
                        break;
                    }
                }
            }
            
            //Load enemies
            for (int i = 0; i < mapData.Layers[enemyIndex].Objects.Length; i++)
            {
                ObjectInfo enemyObject = mapData.Layers[enemyIndex].Objects[i];

                CircleF area = new CircleF(enemyObject.X, enemyObject.Y, 6f);

                Enemy enemy = new Enemy(map);
                enemy.SetPhysicsBody(BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(area.Radius), 88.5f));
                enemy.PhysicsBody.Position = ConvertUnits.ToSimUnits(area.Position);
                enemy.PhysicsBody.CollisionCategories = Category.Cat2;
                enemy.Area = area;
                enemy.CurrentHealth = 10f;

                map.EnemyCount++;
            }
            
            //Load objectives
            for (int i = 0; i < mapData.Layers[objectiveIndex].Objects.Length; i++)
            {
                ObjectInfo areaInfo = mapData.Layers[objectiveIndex].Objects[i];
                RectangleF area = new RectangleF(areaInfo.X, areaInfo.Y, areaInfo.Width, areaInfo.Height);

                const int scientistCount = 5;
                for (int k = 0; k < scientistCount; k++)
                {
                    Vector2 position = new Vector2(RNG.NextFloat(area.Left, area.Right), area.Bottom - 2f);
                    Scientist scientist = new Scientist(map, position, area);

                    scientist.SetPhysicsBody(BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(1f), 5f));
                    scientist.PhysicsBody.Position = ConvertUnits.ToSimUnits(position);
                    //scientist.PhysicsBody.BodyType = BodyType.Dynamic;
                    scientist.PhysicsBody.IsSensor = true;
                }

                map.ScientistCount += scientistCount;
            }

            List<StaticGeometry> waterBodies = new List<StaticGeometry>();

            //Set water zones
            for (int i = 0; i < mapData.Layers[waterIndex].Objects.Length; i++)
            {
                ObjectInfo waterObject = mapData.Layers[waterIndex].Objects[i];
                //if (waterObject.Polygon == null) continue;

                Vertices simGeo = new Vertices(waterObject.Polygon.Length);
                StaticGeometry waterBody = new StaticGeometry(waterObject.Polygon.Length * 2);

                //Loop through the object's vertices (Vector2)
                for (int k = 0; k < waterObject.Polygon.Length; k++)
                {
                    simGeo.Add(ConvertUnits.ToSimUnits(waterObject.Polygon[k]));

                    waterBody.VertexData[k * 2 + 0] = waterObject.Polygon[k].X + waterObject.X;
                    waterBody.VertexData[k * 2 + 1] = waterObject.Polygon[k].Y + waterObject.Y;
                }

                Body geoBody = BodyFactory.CreatePolygon(world, simGeo, 1f);
                geoBody.Position = ConvertUnits.ToSimUnits(new Vector2(waterObject.X, waterObject.Y));
                geoBody.UserData = "Water";
                geoBody.CollisionCategories = Category.Cat3;
                geoBody.IsSensor = true;
                geoBody.OnCollision += (a, b, contact) =>
                {
                    b.Body.GravityScale = 0.05f;
                    return true;
                };
                geoBody.OnSeparation += (a, b) =>
                {
                    b.Body.GravityScale = 1f;
                };

                waterBody.PhysicsBody = geoBody;
                waterBodies.Add(waterBody);
            }
            
            map.StaticGeometry = staticLevelGeometry;
            map.WaterBodies = waterBodies;

            return map;
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
        public string Type;

        public ObjectInfo[] Objects;

        public override string ToString()
        {
            return string.Format("'{0}' - {1}", Name, Type);
        }
    }
    public class ObjectInfo
    {
        public string Name;
        public string Type;
        public int ID;
        public float X, Y;
        public float Width, Height;
        public Vector2[] Polygon;

        public Dictionary<string, object> Properties;
    }
}
