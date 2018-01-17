using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using IronCore.Entities;

namespace IronCore.Utils.Content
{
    public class MapLoader
    {
        private World world;
        private Map map;

        public MapLoader()
        {
            world = new World(new Vector2(0f, 9.8f));
            map = new Map(world);
        }

        public Map LoadMap(MapFile jsonData)
        {
            //Loop through each layer and load appropriate info
            for (int i = 0; i < jsonData.Layers.Length; i++)
            {
                string layerName = jsonData.Layers[i].Name;
                string layerType = jsonData.Layers[i].Type;

                if (!layerType.Equals("objectgroup", StringComparison.OrdinalIgnoreCase)) continue;

                //Start, Water, etc.
                if (layerName.Equals(ZONES_LAYER_NAME, StringComparison.OrdinalIgnoreCase))
                    loadZonesLayer(jsonData.Layers[i]);

                //Scientists
                if (layerName.Equals(OBJECTIVES_LAYER_NAME, StringComparison.OrdinalIgnoreCase))
                    loadObjectivesLayer(jsonData.Layers[i]);
                
                //Placed enemies and enemy spawners
                if (layerName.Equals(ENEMIES_LAYER_NAME, StringComparison.OrdinalIgnoreCase))
                    loadEnemyLayer(jsonData.Layers[i]);

                //Gates and sensors
                if (layerName.Equals(GATES_LAYER_NAME, StringComparison.OrdinalIgnoreCase))
                    loadGateLayer(jsonData.Layers[i]);

                //Static collision geometry
                if (layerName.Equals(COLLISION_LAYER_NAME, StringComparison.OrdinalIgnoreCase))
                    loadCollisionLayer(jsonData.Layers[i]);
            }

            //TODO: Organize and figure out collision categories
            return map;
        }

        private void loadCollisionLayer(LayerInfo layer)
        {
            List<StaticGeometry> staticLevelGeometry = new List<StaticGeometry>();

            //Load static geometry
            for (int i = 0; i < layer.Objects.Length; i++)
            {
                ObjectInfo collisionObject = layer.Objects[i];
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

            map.StaticGeometry = staticLevelGeometry;
        }
        private void loadGateLayer(LayerInfo layer)
        {
            //Load Gates and Sensors
            for (int i = 0; i < layer.Objects.Length; i++)
            {
                ObjectInfo mapObject = layer.Objects[i];

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
                    gate.IsImmortal = true;

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

                    map.Entities.Add(gate);
                }
                else if (mapObject.Type.Equals("Sensor"))
                {
                    physicsBody.IsSensor = true;
                    physicsBody.CollisionCategories = Category.Cat3;

                    Sensor sensor = new Sensor(map);
                    sensor.SetPhysicsBody(physicsBody);
                    sensor.TargetGateName = mapObject.Properties.ContainsKey("Gate") ? (string)mapObject.Properties["Gate"] : "None";
                    sensor.DisplayArea = area;
                    sensor.IsImmortal = true;
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
        }
        private void loadEnemyLayer(LayerInfo layer)
        {
            //Load enemies
            for (int i = 0; i < layer.Objects.Length; i++)
            {
                ObjectInfo enemyObject = layer.Objects[i];

                CircleF area = new CircleF(enemyObject.X, enemyObject.Y, 6f);
                
                Enemy enemy = new Enemy(map);
                enemy.SetPhysicsBody(BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(area.Radius), 88.5f));
                enemy.PhysicsBody.Position = ConvertUnits.ToSimUnits(area.Position);
                enemy.PhysicsBody.CollisionCategories = Category.Cat1;
                enemy.Area = area;
                enemy.MaxHealth = 10f;

                map.EnemyCount++;
            }
        }
        private void loadObjectivesLayer(LayerInfo layer)
        {
            //Load objectives
            for (int i = 0; i < layer.Objects.Length; i++)
            {
                ObjectInfo areaInfo = layer.Objects[i];
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
                    scientist.IsImmortal = true; //TODO: Consider killable scientists
                }

                map.ScientistCount += scientistCount;
            }
        }
        private void loadZonesLayer(LayerInfo layer)
        {
            List<StaticGeometry> waterBodies = new List<StaticGeometry>();

            //Set water zones
            for (int i = 0; i < layer.Objects.Length; i++)
            {
                ObjectInfo zoneObject = layer.Objects[i];
                if (zoneObject.Type.Equals("Water", StringComparison.OrdinalIgnoreCase))
                {
                    if (zoneObject.Polygon == null) continue;

                    Vertices simGeo = new Vertices(zoneObject.Polygon.Length);
                    StaticGeometry waterBody = new StaticGeometry(zoneObject.Polygon.Length * 2);

                    //Loop through the object's vertices (Vector2)
                    for (int k = 0; k < zoneObject.Polygon.Length; k++)
                    {
                        simGeo.Add(ConvertUnits.ToSimUnits(zoneObject.Polygon[k]));

                        waterBody.VertexData[k * 2 + 0] = zoneObject.Polygon[k].X + zoneObject.X;
                        waterBody.VertexData[k * 2 + 1] = zoneObject.Polygon[k].Y + zoneObject.Y;
                    }

                    Body geoBody = BodyFactory.CreatePolygon(world, simGeo, 1f);
                    geoBody.Position = ConvertUnits.ToSimUnits(new Vector2(zoneObject.X, zoneObject.Y));
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
                else if (zoneObject.Type.Equals("PlayerStart", StringComparison.OrdinalIgnoreCase))
                {
                    Vector2 playerStart = new Vector2();
                    playerStart.X = (zoneObject.X + zoneObject.X + zoneObject.Width) / 2f;
                    playerStart.Y = zoneObject.Y + zoneObject.Height; //Player height is adjusted from the bottom

                    map.PlayerStart = playerStart;
                }
            }

            map.WaterBodies = waterBodies;
        }

        private const string ZONES_LAYER_NAME       = "Zones";
        private const string OBJECTIVES_LAYER_NAME  = "Objectives";
        private const string WATER_LAYER_NAME       = "Water";
        private const string ENEMIES_LAYER_NAME     = "Enemies";
        private const string GATES_LAYER_NAME       = "Gates";
        private const string COLLISION_LAYER_NAME   = "Collision";
        private const string OBJECT_LAYER_TYPE      = "objectgroup";
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
            return $"'{Name}' - {Type}";
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
