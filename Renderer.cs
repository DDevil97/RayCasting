using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SFMLTest.RayCaster;
using Sprite = SFMLTest.RayCaster.Sprite;
using static SFMLTest.Helpers;
using System.Threading;

namespace SFMLTest
{
    class Renderer
    {
        #region Properties
        private float HalfFov;
        private float DistanceToProjectionPlane;
        private float[] Angles;
        private float[] DepthPerStrip;
        private Color[,] LightMap;
        private float LightMultiplier;

        public float Fov { get; set; }
        public int LightMapScaler { get; set; }
        public int MapAtlasInUse { get; set; }
        public Color AmbientLight { get; set; }
        public RayCaster Caster { get; set; }
        public RenderTexture Buffer { get; set; }
        public List<Texture> Textures { get; set; }
        public Vector2f AtlasTileSize { get; set; } 
        #endregion

        private float calculateLampIntensityAtPoint(Vector2f mPos, Vector2f lPos)
        {
            RayResult r = Caster.RayCast(lPos, Atan2D(mPos.Y - lPos.Y, mPos.X - lPos.X));
            if (r.Magnitude + 0.1 >= Distance(mPos, lPos))
                return 1f / ((float)Math.Pow(Distance(mPos, lPos), 2) / LightMultiplier);
            else
                return 0;
        }

        private List<Vertex> calculateFloor(Vector2f player, float angle, int yFrom, int yTo, ref List<Vertex> points)
        {
            if (yTo > Buffer.Size.Y)
                yTo = (int)Buffer.Size.Y;

            for (int y = yFrom; y < yTo; y++) 
            {              
                float dist = ((Caster.CellSize / (y - Buffer.Size.Y / 2f)) * DistanceToProjectionPlane / 2f) / CosD(HalfFov);
                Vector2f left = player + new Vector2f(dist * CosD(angle - HalfFov), dist * SinD(angle - HalfFov));
                Vector2f right = player + new Vector2f(dist * CosD(angle + HalfFov), dist * SinD(angle + HalfFov));
                Vector2f delta = (right - left) / Buffer.Size.X;

                for (int x = 0; x < Buffer.Size.X; x++)
                {

                    int pX = (int)(left.X * LightMapScaler);
                    if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0) - 1;
                    if (pX < 0) pX = 0;

                    int pY = (int)(left.Y * LightMapScaler);
                    if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                    if (pY < 0) pY = 0;

                    TileInfo t = Caster.GetMap(Floor(left.X / Caster.CellSize), Floor(left.Y / Caster.CellSize));
                    points.Add(new Vertex
                    {
                        Position = new Vector2f(x, y),
                        TexCoords = new Vector2f(t.FloorAtlas.X * Caster.CellSize + (left.X % Caster.CellSize), t.FloorAtlas.Y * Caster.CellSize + left.Y % Caster.CellSize),
                        Color = LightMap[pX, pY]
                    });

                    points.Add(new Vertex
                    {
                        Position = new Vector2f(x, Buffer.Size.Y - y),
                        TexCoords = new Vector2f(t.CeilAtlas.X * Caster.CellSize + (left.X % Caster.CellSize), t.CeilAtlas.Y * Caster.CellSize + left.Y % Caster.CellSize),
                        Color = LightMap[pX, pY]
                    });

                    left += delta;
                }
            }

            return points;
        }

        public Renderer(RayCaster rc, RenderTexture rt, float fov)
        {
            Caster = rc;
            Buffer = rt;
            Fov = fov;
            HalfFov = fov / 2;
            DistanceToProjectionPlane = (rt.Size.X / 2) / TanD(fov / 2);
            Textures = new List<Texture>();
            DepthPerStrip = new float[Buffer.Size.X];
            AtlasTileSize = new Vector2f(Caster.CellSize, Caster.CellSize);
            AmbientLight = new Color(32, 32, 32);
            Angles = new float[Buffer.Size.X];
            LightMultiplier = 400;

            for (int x = 0; x < Buffer.Size.X; x++)
                Angles[x] = AtanD((x - Buffer.Size.X / 2.0f) / DistanceToProjectionPlane);
        }

        public void GenerateLightMap(List<Light> lamps)
        {
            LightMap = new Color[Caster.CellSize * Caster.Map.GetLength(0) * LightMapScaler, Caster.CellSize * Caster.Map.GetLength(1) * LightMapScaler];
            for (int y = 0; y < LightMap.GetLength(1); y++)
                for (int x = 0; x < LightMap.GetLength(0); x++)
                    LightMap[x,y] = AmbientLight;

                    for (int y = 0; y < LightMap.GetLength(1); y++)
                for (int x = 0; x < LightMap.GetLength(0); x++)
                {
                    float r = 0;
                    float g = 0;
                    float b = 0;
                    foreach (Light l in lamps)
                    {
                        float i = calculateLampIntensityAtPoint(new Vector2f((float)x / LightMapScaler, (float)y / LightMapScaler), l.Position);
                        r += i * l.Color.R;
                        g += i * l.Color.G;
                        b += i * l.Color.B;
                    }
                    LightMap[x, y] += new Color(Clamp<byte>(r, 0, 255), Clamp<byte>(g, 0, 255), Clamp<byte>(b, 0, 255));

                    LightMap[Clamp<int>(x - 1, 0, LightMap.GetLength(0) - 1), Clamp<int>(y, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));
                    LightMap[Clamp<int>(x + 1, 0, LightMap.GetLength(0) - 1), Clamp<int>(y, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));
                    LightMap[Clamp<int>(x, 0, LightMap.GetLength(0) - 1), Clamp<int>(y + 1, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));
                    LightMap[Clamp<int>(x, 0, LightMap.GetLength(0) - 1), Clamp<int>(y - 1, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));


                }
        }

        public void Render(Vector2f player, float angle, List<Sprite> sprites)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<Vertex> spritesLines = new List<Vertex>();
            List<Vertex> points = new List<Vertex>();

            int MinLineHeight = (int)Buffer.Size.Y;

            for (int x = 0; x < Buffer.Size.X; x++)
            {
                float rayAngle = Angles[x];
                RayResult ray = Caster.RayCast(player, angle + rayAngle);
                DepthPerStrip[x] = ray.Magnitude * CosD(rayAngle);
                int lineHeightHalf = Floor((Caster.CellSize / DepthPerStrip[x]) * DistanceToProjectionPlane) >> 1;
                if (lineHeightHalf < MinLineHeight)
                    MinLineHeight = lineHeightHalf;

                TileInfo t = Caster.GetMap(ray.Tile.X, ray.Tile.Y);

                Vector2f textureCordUp;
                Vector2f textureCordDown;
                switch (ray.Side)
                {
                    case Side.Down:
                        textureCordUp = new Vector2f(
                            t.DownAtlas.X * Caster.CellSize + (ray.Position.X % Caster.CellSize),
                            t.DownAtlas.Y * Caster.CellSize);
                        textureCordDown = new Vector2f(
                            textureCordUp.X,
                            textureCordUp.Y + Caster.CellSize);
                        break;
                    case Side.Up:
                        textureCordUp = new Vector2f(
                            t.UpAtlas.X * Caster.CellSize + (ray.Position.X % Caster.CellSize),
                            t.UpAtlas.Y * Caster.CellSize);
                        textureCordDown = new Vector2f(
                            textureCordUp.X,
                            textureCordUp.Y + Caster.CellSize);
                        break;
                    case Side.Left:
                        textureCordUp = new Vector2f(
                            t.LeftAtlas.X * Caster.CellSize + (ray.Position.Y % Caster.CellSize),
                            t.LeftAtlas.Y * Caster.CellSize);
                        textureCordDown = new Vector2f(
                            textureCordUp.X,
                            textureCordUp.Y + Caster.CellSize);
                        break;
                    case Side.Right:
                        textureCordUp = new Vector2f(
                            t.RightAtlas.X * Caster.CellSize + (ray.Position.Y % Caster.CellSize),
                            t.RightAtlas.Y * Caster.CellSize);
                        textureCordDown = new Vector2f(
                            textureCordUp.X,
                            textureCordUp.Y + Caster.CellSize);
                        break;
                    default:
                        textureCordUp = new Vector2f(
                            (ray.Position.Y % Caster.CellSize),
                            t.RightAtlas.Y * Caster.CellSize);
                        textureCordDown = new Vector2f(
                            textureCordUp.X,
                            textureCordUp.Y + Caster.CellSize - 1);
                        break;
                }

                int pX = (int)(ray.Position.X * LightMapScaler);
                if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0) - 1;
                if (pX < 0) pX = 0;

                int pY = (int)(ray.Position.Y * LightMapScaler);
                if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                if (pY < 0) pY = 0;

                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Buffer.Size.Y / 2 - lineHeightHalf),
                    Color = LightMap[pX, pY],
                    TexCoords = textureCordUp
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Buffer.Size.Y / 2 + lineHeightHalf),
                    Color = LightMap[pX, pY],
                    TexCoords = textureCordDown
                });
            }

            int chunkCount = 4;
            int start = Floor(Buffer.Size.Y / 2) + MinLineHeight;
            int chunkSize = ((int)Buffer.Size.Y - start) / chunkCount;

            List<Vertex>[] a = new List<Vertex>[chunkCount];
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i <chunkCount; i++)
            {
                a[i] = new List<Vertex>();
                int b = i;
                Thread t = new Thread(new ThreadStart(() => calculateFloor(player, angle, start + chunkSize * b, start + chunkSize * b + chunkSize,ref a[b])));
                threads.Add(t);
            }

            foreach (Thread t in threads)
                t.Start();

            while (threads.Count(t => t.ThreadState == ThreadState.Running) > 0) ;

            foreach (List<Vertex> l in a)
               if (l != null)
                    points.AddRange(l);


            List<Sprite> finalList = new List<Sprite>();
            foreach (Sprite spr in sprites)
            {
                int pX = (int)(spr.Position.X * LightMapScaler);
                if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0) - 1;
                if (pX < 0) pX = 0;

                int pY = (int)(spr.Position.Y * LightMapScaler);
                if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                if (pY < 0) pY = 0;

                spr.Light = LightMap[pX, pY];

                spr.Position = Rotate(spr.Position - player, -angle);
                spr.Distance = spr.Position.X;
                if (spr.Distance > 0)
                    finalList.Add(spr);
            }

            finalList = finalList.OrderByDescending(s => s.Position.X).ToList();

            foreach (Sprite spr in finalList)
            {

                int lineHeight = (int)((Caster.CellSize / spr.Distance) * DistanceToProjectionPlane);
                int px = (int)(Buffer.Size.X / 2 + (spr.Position.Y / spr.Distance) * DistanceToProjectionPlane);


                for (int x = 0; x < lineHeight; x++)
                {
                    int posX = (px + lineHeight / 2) - x;

                    if (posX >= 0 && posX < Buffer.Size.X && DepthPerStrip[posX] > spr.Distance)
                    {
                        float tex = ((float)x / lineHeight) * Caster.CellSize;

                        Vector2f textureCordUp = new Vector2f(
                            (spr.Atlas.X + 1) * Caster.CellSize - tex,
                            spr.Atlas.Y * Caster.CellSize);

                        Vector2f textureCordDown = new Vector2f(
                            textureCordUp.X,
                            textureCordUp.Y + Caster.CellSize);

                        spritesLines.Add(new Vertex
                        {
                            Position = new Vector2f(posX, Buffer.Size.Y/2 - lineHeight / 2),
                            Color = spr.Light,
                            TexCoords = textureCordUp
                        });
                        spritesLines.Add(new Vertex
                        {
                            Position = new Vector2f(posX, Buffer.Size.Y /2 + lineHeight / 2),
                            Color = spr.Light,
                            TexCoords = textureCordDown
                        });
                    }
                }
            }

            Buffer.Draw(points.ToArray(), 0, (uint)points.Count, PrimitiveType.Points, new RenderStates(Textures[MapAtlasInUse]));
            Buffer.Draw(vertices.ToArray(), 0, (uint)vertices.Count, PrimitiveType.Lines, new RenderStates(Textures[MapAtlasInUse]));
            Buffer.Draw(spritesLines.ToArray(), 0, (uint)spritesLines.Count, PrimitiveType.Lines, new RenderStates(Textures[MapAtlasInUse]));
        }
    }
}
