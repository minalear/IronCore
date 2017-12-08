﻿using System.Globalization;
using OpenTK;
using OpenTK.Graphics;

namespace IronCore.Utils
{
    public static class Extensions
    {
        /*NUMBER PARSING*/
        public static float ParseFloat(this string floatStr)
        {
            return float.Parse(floatStr);
        }
        public static int ParseInt(this string intStr)
        {
            return int.Parse(intStr, CultureInfo.InvariantCulture.NumberFormat);
        }

        /*ARRAY BYTE SIZING*/
        public static int SizeOf(this float[] array)
        {
            return array.Length * sizeof(float);
        }
        public static int SizeOf(this int[] array)
        {
            return array.Length * sizeof(int);
        }

        /*VECTOR TO ARRAY*/
        public static float[] ToArray(this Vector2 v)
        {
            return new float[] { v.X, v.Y };
        }
        public static float[] ToArray(this Vector3 v)
        {
            return new float[] { v.X, v.Y, v.Z };
        }
        public static float[] ToArray(this Vector4 v)
        {
            return new float[] { v.X, v.Y, v.Z, v.W };
        }

        /// <summary>
        /// Normalizes a Vector, but ensures the length is not zero first.
        /// </summary>
        public static Vector2 SafeNormalize(this Vector2 v)
        {
            if (v.LengthSquared != 0f)
            {
                v.Normalize();
                return v;
            }
            return Vector2.Zero;
        }
        public static Vector2d SafeNormalize(this Vector2d v)
        {
            if (v.LengthSquared != 0f)
            {
                v.Normalize();
                return v;
            }
            return Vector2d.Zero;
        }

        /*COLORS*/
        public static Vector3 RGB(this Color4 color)
        {
            return new Vector3(color.R, color.G, color.B);
        }
    }
}