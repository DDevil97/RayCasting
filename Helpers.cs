using SFML.Graphics;
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
        public static byte Clamp(float num) => (byte)((num < 0) ? 0 : (num > 255) ? 255 : num);
        public static int Clamp(int num, int from, int to) => (num < from) ? from : (num > to) ? to : num;

        public static Color ColorAdd(Color lhs, Color rhs)
        {
            int r = lhs.R + rhs.R;
            if (r > 255)
                r = 255;

            int g = lhs.G + rhs.G;
            if (g > 255)
                g = 255;

            int b = lhs.B + rhs.B;
            if (b > 255)
                b = 255;

            return new Color(Clamp(r),Clamp(g),Clamp(b));
        }

        // Find the points of intersection.
        public static int LineCircleIntersections(float cx, float cy, float radius,
                                                  Vector2f point1, Vector2f point2,
                                                  out Vector2f intersection1, out Vector2f intersection2)
        {
            float dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) +
                (point1.Y - cy) * (point1.Y - cy) -
                radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new Vector2f(float.NaN, float.NaN);
                intersection2 = new Vector2f(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 =
                    new Vector2f(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new Vector2f(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 =
                    new Vector2f(point1.X + t * dx, point1.Y + t * dy);
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 =
                    new Vector2f(point1.X + t * dx, point1.Y + t * dy);
                return 2;
            }
        }
    }
}
