﻿using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace IronCore.Utils
{
    public class ShaderProgram : IDisposable
    {
        private int programID;
        private Dictionary<string, int> uniformLocations;

        public ShaderProgram(int id)
        {
            programID = id;
            uniformLocations = new Dictionary<string, int>();
        }

        public void Use()
        {
            GL.UseProgram(programID);
        }
        public void Clear()
        {
            GL.UseProgram(0);
        }
        public void SetInt(string name, int value)
        {
            GL.Uniform1(GetUniformLocation(name), value);
        }
        public void SetFloat(string name, float value)
        {
            GL.Uniform1(GetUniformLocation(name), value);
        }
        public void SetVector2(string name, Vector2 value)
        {
            GL.Uniform2(GetUniformLocation(name), value);
        }
        public void SetVector3(string name, Vector3 value)
        {
            GL.Uniform3(GetUniformLocation(name), value);
        }
        public void SetVector4(string name, Vector4 value)
        {
            GL.Uniform4(GetUniformLocation(name), value);
        }
        public void SetColor4(string name, Color4 value)
        {
            GL.Uniform4(GetUniformLocation(name), value);
        }
        public void SetMatrix4(string name, bool transpose, Matrix4 value)
        {
            GL.UniformMatrix4(GetUniformLocation(name), transpose, ref value);
        }

        public void SetMatrix4Array(string name, bool transpose, Matrix4[] values)
        {
            for (int i = 0; i < values.Length; i++)
                SetMatrix4($"{name}[{i}]", transpose, values[i]);
        }

        public int GetUniformLocation(string name)
        {
            if (uniformLocations.ContainsKey(name))
                return uniformLocations[name];

            int location = GL.GetUniformLocation(ID, name);
            if (location != -1)
                uniformLocations.Add(name, location);
            return location;
        }

        public void Dispose()
        {
            GL.DeleteProgram(programID);
        }

        public int ID { get { return programID; } }
    }
}
