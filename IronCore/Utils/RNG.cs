using System;
using OpenTK.Graphics;

namespace IronCore.Utils
{
    public static class RNG
    {
        private static Random randomGenerator = new Random(Guid.NewGuid().GetHashCode());

        public static void Init()
        {
            for (int i = 0; i < 512; i++)
            {
                p[i] = perlinPermutation[i % 256];
            }
        }

        /// <summary>
        /// Returns a random integer number.
        /// </summary>
        public static int Next()
        {
            return randomGenerator.Next();
        }

        /// <summary>
        /// Returns a random integer number with an upper limit.
        /// </summary>
        public static int Next(int max)
        {
            return randomGenerator.Next(max);
        }

        /// <summary>
        /// Returns a random integer number between two values.
        /// </summary>
        public static int Next(int min, int max)
        {
            return randomGenerator.Next(min, max);
        }

        /// <summary>
        /// Returns a random double number.
        /// </summary>
        public static double NextDouble()
        {
            return randomGenerator.NextDouble();
        }

        /// <summary>
        /// Returns a random double number between two values.
        /// </summary>
        public static double NextDouble(double min, double max)
        {
            return NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Returns a random floating-point number between 0.0 and 1.0.
        /// </summary>
        public static float NextFloat()
        {
            return (float)randomGenerator.NextDouble();
        }

        /// <summary>
        /// Returns a random floating-point number between two values.
        /// </summary>
        public static float NextFloat(float min, float max)
        {
            return NextFloat() * (max - min) + min;
        }

        /// <summary>
        /// Returns a random color with full opacity
        /// </summary>
        public static Color4 NextColor()
        {
            float r = NextFloat(0.25f, 1f);
            float g = NextFloat(0.25f, 1f);
            float b = NextFloat(0.25f, 1f);

            return new Color4(r, g, b, 1.0f);
        }

        public static float Perlin(float x, float y, float z)
        {
            int xi = (int)x & 255;

            return 1.0f;
        }

        // Hash lookup table as defined by Ken Perlin.  This is a randomly
        // arranged array of all numbers from 0-255 inclusive.
        private static readonly int[] perlinPermutation = { 151,160,137,91,90,15,                 
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,    
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };
        private static readonly int[] p = new int[512];
    }
}
