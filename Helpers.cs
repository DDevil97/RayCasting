using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLTest
{
    class Helpers
    {
        #region Constants
        public const float DegRad = 3.14159265359f / 180.0f;
        public const float RadDeg = 180.0f / 3.14159265359f;
        #endregion

        public static float TanD(float A) => (float)Math.Tan(A * DegRad);
        public static float CosD(float A) => (float)Math.Cos(A * DegRad);
        public static float SinD(float A) => (float)Math.Sin(A * DegRad);
        public static float Atan2D(float y, float x) => (float)Math.Atan2(y, x) / DegRad;
        public static float AtanD(float L) => (float)Math.Atan(L) / DegRad;
        public static float Distance(Vector2f a, Vector2f b) => (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        public static int Floor(float N) => (int)Math.Floor(N);
        public static int Ceil(float N) => (int)Math.Ceiling(N);
        public static int Round(float N) => (int)Math.Round(N);
        public static T Clamp<T>(dynamic num, dynamic bottom, dynamic top) => (num < bottom) ? (T)bottom : (num > top) ? (T)top : (T)num;
    }
}
