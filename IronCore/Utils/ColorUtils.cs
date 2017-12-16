using OpenTK;
using OpenTK.Graphics;

namespace IronCore.Utils
{
    public static class ColorUtils
    {
        /// <summary>
        /// Blend two colors given by a given amount.
        /// </summary>
        /// <param name="a">Primary Color</param>
        /// <param name="b">Secondary Color</param>
        /// <param name="factor">1.0 - Fully Primary Color. 0.0 - Fully Secondary Color</param>
        /// <returns></returns>
        public static Color4 Blend(Color4 a, Color4 b, float factor)
        {
            float aFactor = factor;
            float bFactor = 1f - factor;

            Vector4 vecA = new Vector4(a.R, a.G, a.B, a.A);
            Vector4 vecB = new Vector4(b.R, b.G, b.B, b.A);

            Vector4 blend = ((vecA * aFactor) + (vecB * bFactor)) / 2f;
            return new Color4(blend.X, blend.Y, blend.Z, blend.W);
        }
    }
}
