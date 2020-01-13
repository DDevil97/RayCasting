using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.System;

namespace SFMLTest
{
    class RayCaster
    {
        #region Constants
        const float DegRad = 3.14159265359f / 180.0f;
        const int MaxDistance = 60000;
        #endregion

        #region Properties
        public int CellSize { get; set; }

        public TileInfo[,] Map { get; set; } 
        #endregion

        #region Clases
        public enum Side
        {
            Up = 0,
            Right,
            Down,
            Left
        }

        public class RayResult
        {
            public Vector2i Tile { get; set; }
            public Vector2f Position { get; set; }
            public float Magnitude { get; set; }
            public Side Side { get; set; }
        } 

        public class TileInfo
        {
            public bool Solid { get; set; }
            public Vector2i UpAtlas { get; set; }
            public Vector2i DownAtlas { get; set; }
            public Vector2i LeftAtlas { get; set; }
            public Vector2i RightAtlas { get; set; }
        }
        #endregion

        public static float TanD(float A) => (float)Math.Tan(A * DegRad);
        public static float CosD(float A) => (float)Math.Cos(A * DegRad);
        public static float SinD(float A) => (float)Math.Sin(A * DegRad);
        public static float AtanD(float L) => (float)Math.Atan(L) / DegRad;
        public static int Floor(float N) => (int)Math.Floor(N);

        public TileInfo GetMap(int px, int py)
        {
            if (px < 0 || px > Map.GetLength(0) - 1 || py < 0 || py > Map.GetLength(1) - 1)
                return new TileInfo { Solid = true};
            else
                return Map[px, py];
        }

        public RayResult RayCast(Vector2f O, float A)
        {
            Vector2f D = O + new Vector2f((CosD(A) * 100), (SinD(A) * 100));
            Vector2f Slope = D - O;
            Vector2f Delta = new Vector2f();
            Vector2f Ph = new Vector2f(), Pv = new Vector2f();
            Vector2i Th, Tv;
            float Dh, Dv;
            RayResult res;

            //If Dx is zero, then there's no horizontal intersections
            if (Slope.X == 0)
            {
                Th = new Vector2i(-1,-1);
                Ph = new Vector2f(O.X + MaxDistance, O.Y + MaxDistance);
                Dh = MaxDistance;
            }
            else
            {
                Delta.X = CellSize * Math.Sign(Slope.X);

                Ph.X = Floor(O.X / CellSize);
                Ph.X = (Ph.X + (Math.Sign(Delta.X) == 1 ? 1 : 0)) * CellSize;

                if (Slope.Y == 0)
                {
                    Delta.Y = 0;
                    Ph.Y = O.Y;
                }
                else
                {
                    Delta.Y = (Delta.X * Slope.Y) / Slope.X;
                    Ph.Y = O.Y + (Slope.Y * (Ph.X - O.X)) / Slope.X;
                }

                while (!GetMap(Floor(Ph.X / CellSize) + (Delta.X < 0 ? -1 : 0), Floor(Ph.Y / CellSize)).Solid)
                    Ph += Delta;

                Th = new Vector2i(Floor(Ph.X / CellSize) + (Delta.X < 0 ? -1 : 0), Floor(Ph.Y / CellSize));
                Dh = (float)Math.Sqrt(Math.Pow((Ph.X - O.X), 2) + Math.Pow((Ph.Y - O.Y), 2));
            }

            //If Dy is zero, then there's no vertical intersections
            if (Slope.Y == 0)
            {
                Tv = new Vector2i(-1, -1);
                Pv = new Vector2f(O.X + MaxDistance, O.Y + MaxDistance);
                Dv = MaxDistance;
            }
            else
            {
                Delta.Y = CellSize * Math.Sign(Slope.Y);

                Pv.Y = Floor(O.Y / CellSize);
                Pv.Y = (Pv.Y + (Math.Sign(Delta.Y) == 1 ? 1 : 0)) * CellSize;

                if (Slope.X == 0)
                {
                    Delta.X = 0;
                    Pv.X = O.X;
                }
                else
                {
                    Delta.X = (Delta.Y * Slope.X) / Slope.Y;
                    Pv.X = O.X + (Slope.X * (Pv.Y - O.Y)) / Slope.Y;
                }


                while (!GetMap(Floor(Pv.X / CellSize), Floor(Pv.Y / CellSize) + (Delta.Y <0 ? -1 : 0)).Solid)
                    Pv += Delta;

                Tv = new Vector2i(Floor(Pv.X / CellSize), Floor(Pv.Y / CellSize) + (Delta.Y < 0 ? -1 : 0));
                Dv = (float)Math.Sqrt(Math.Pow(Pv.X - O.X, 2) + Math.Pow(Pv.Y - O.Y, 2));
            }


            if (Dh < Dv)
            {
                res = new RayResult
                {
                    Tile = Th,
                    Position = Ph,
                    Magnitude = Dh,
                    Side = Slope.X < 0 ? Side.Left : Side.Right
                };

                return res;
            }
            else
            {
                res = new RayResult
                {
                    Tile = Tv,
                    Position = Pv,
                    Magnitude = Dv,
                    Side = Slope.Y < 0 ? Side.Up : Side.Down
                };

                return res;
            }
        }

    }
}
