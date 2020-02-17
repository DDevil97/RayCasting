using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFMLTest.Tile;
//using static SFMLTest.Helpers;
using static MathFloat.MathF;

namespace SFMLTest
{
    class RayCaster
    {
        const int MaxDistance = 60000;

        #region Properties
        public int CellSize { get; set; }
        public BaseTile[,] Map { get; set; }
        #endregion

        #region Clases
        public enum Side
        {
            Up = 0,
            Right,
            Down,
            Left,
            None
        }

        public class RayResult
        {
            public bool Valid { get; set; }
            public Vector2i Tile { get; set; }
            public Vector2f Position { get; set; }
            public Side Side { get; set; }
            public float Magnitude { get; set; }
        }
        #endregion

        public static Vector2f RotateAround(Vector2f vector, Vector2f origin, float angle)
        {
            angle *= Helpers.DegRad;
            vector -= origin;
            return new Vector2f(
                Cos(angle) * vector.X - Sin(angle) * vector.Y, 
                Sin(angle) * vector.X + Cos(angle) * vector.Y) 
                + origin;
        }

        public static Vector2f Rotate(Vector2f vector, float angle)
        {
            angle *= Helpers.DegRad;
            return new Vector2f(
                Cos(angle) * vector.X - Sin(angle) * vector.Y,
                Sin(angle) * vector.X + Cos(angle) * vector.Y);
        }

        public BaseTile GetMap(int px, int py, Vector2i toIgnore = new Vector2i())
        {
            if (px == toIgnore.X & py == toIgnore.Y)
                return new BaseTile { Solid = false };

            if (px < 0 || px > Map.GetLength(0) - 1 || py < 0 || py > Map.GetLength(1) - 1)
                return new BaseTile { Solid = true };
            else
                return Map[px, py];
        }

        private RayResult RayCastInternal(Vector2f O, Vector2f startO, float A, Vector2i toIgnore)
        {
            A *= Helpers.DegRad;
            Vector2f D = startO + new Vector2f(Cos(A) * 1000, Sin(A) * 1000);
            Vector2f Slope = D - startO;
            Vector2f Delta = new Vector2f();
            Vector2f Ph = new Vector2f(), Pv = new Vector2f();
            float Dh, Dv;
            RayResult res;

            //If Dx is zero, then there's no horizontal intersections
            if (Slope.X == 0)
            {
                Ph = new Vector2f(startO.X + MaxDistance, startO.Y + MaxDistance);
                Dh = MaxDistance;
            }
            else
            {
                Delta.X = CellSize * Math.Sign(Slope.X);

                Ph.X = Floor(startO.X / CellSize);
                Ph.X = (Ph.X + (Math.Sign(Delta.X) == 1 ? 1 : 0)) * CellSize;

                if (Slope.Y == 0)
                {
                    Delta.Y = 0;
                    Ph.Y = startO.Y;
                }
                else
                {
                    Delta.Y = (Delta.X * Slope.Y) / Slope.X;
                    Ph.Y = startO.Y + (Slope.Y * (Ph.X - startO.X)) / Slope.X;
                }

                while (!GetMap(Helpers.Floor(Ph.X / CellSize) + (Delta.X < 0 ? -1 : 0), Helpers.Floor(Ph.Y / CellSize), toIgnore).Solid)
                    Ph += Delta;

                Dh = Sqrt(Pow((Ph.X - O.X), 2) + Pow((Ph.Y - O.Y), 2));
            }

            //If Dy is zero, then there's no vertical intersections
            if (Slope.Y == 0)
            {
                Pv = new Vector2f(startO.X + MaxDistance, startO.Y + MaxDistance);
                Dv = MaxDistance;
            }
            else
            {
                Delta.Y = CellSize * Math.Sign(Slope.Y);

                Pv.Y = Floor(startO.Y / CellSize);
                Pv.Y = (Pv.Y + (Math.Sign(Delta.Y) == 1 ? 1 : 0)) * CellSize;

                if (Slope.X == 0)
                {
                    Delta.X = 0;
                    Pv.X = startO.X;
                }
                else
                {
                    Delta.X = (Delta.Y * Slope.X) / Slope.Y;
                    Pv.X = startO.X + (Slope.X * (Pv.Y - startO.Y)) / Slope.Y;
                }


                while (!GetMap(Helpers.Floor(Pv.X / CellSize), Helpers.Floor(Pv.Y / CellSize) + (Delta.Y < 0 ? -1 : 0), toIgnore).Solid)
                    Pv += Delta;

                Dv = Sqrt(Pow(Pv.X - O.X, 2) + Pow(Pv.Y - O.Y, 2));
            }


            if (Dh < Dv)
            {
                res = new RayResult
                {
                    Tile = new Vector2i(Helpers.Floor(Ph.X / CellSize) + (Delta.X < 0 ? -1 : 0), Helpers.Floor(Ph.Y / CellSize)),
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
                    Tile = new Vector2i(Helpers.Floor(Pv.X / CellSize), Helpers.Floor(Pv.Y / CellSize) + (Delta.Y < 0 ? -1 : 0)),
                    Position = Pv,
                    Magnitude = Dv,
                    Side = Slope.Y < 0 ? Side.Up : Side.Down
                };

                return res;
            }
        }

        public RayResult RayCast(Vector2f O, float A)
        {
            RayResult result;
            Vector2f start = O;
            Vector2i toIgnore = new Vector2i(300000,300000);
            int fs = 0;

            do
            {
                result = RayCastInternal(O,start,A, toIgnore);
                result = GetMap(result.Tile.X, result.Tile.Y, toIgnore).OnIntersection(result, O, A, this);
                start = result.Position;
                toIgnore = result.Tile;
            }
            while (!result.Valid & fs++<15);

            return result;
        }
    }
}
