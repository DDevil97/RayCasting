using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SFMLTest.RayCaster;
using static SFMLTest.Helpers;
using System.Threading;
using SFMLTest.Tile;

namespace SFMLTest
{
    public class Sprite
    {
        public Vector2f RenderPosition { get; set; }

        public Vector2f Position { get; set; }
        public Vector2i Atlas { get; set; }
        public Color Light { get; set; }
        public float Distance { get; set; }
    }

    public class Light
    {
        public Color Color { get; set; }
        public Vector2f Position { get; set; }
    }

    class Renderer
    {
        public bool DynLight = false;

        #region Properties
        private float HalfFov;
        private float DistanceToProjectionPlane;
        private float[] Angles;
        private float[] DepthPerStrip;
        private Color[,] LightMap;
        private float LightMultiplier;
        private Vertex[] DisplayVertices;

        public RenderWindow Screen { get; set; }
        public float Fov { get; set; }
        public float LightMapScaler { get; set; }
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
            if (r.Magnitude +1 >= Distance(mPos, lPos))
                return 1f / ((float)Math.Pow(Distance(mPos, lPos), 2) / LightMultiplier);
            else
                return 0;
        }

        private void calculateFloor(Vector2f player, float angle, int y, ref Vertex[] points, List<Light> lamps)
        {              
                float dist = ((Caster.CellSize / (y - Buffer.Size.Y / 2f)) * DistanceToProjectionPlane / 2f) / CosD(HalfFov);
                Vector2f left = player + new Vector2f(dist * CosD(angle - HalfFov), dist * SinD(angle - HalfFov));
                Vector2f right = player + new Vector2f(dist * CosD(angle + HalfFov), dist * SinD(angle + HalfFov));
                Vector2f delta = (right - left) / Buffer.Size.X;

                for (int x = 0; x < Buffer.Size.X; x++)
                {

                    Color col;

                if (!DynLight)
                {
                    int pX = (int)(left.X * LightMapScaler);
                    if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0) - 1;
                    if (pX < 0) pX = 0;

                    int pY = (int)(left.Y * LightMapScaler);
                    if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                    if (pY < 0) pY = 0;
                    col = LightMap[pX, pY];
                }
                else
                    col = getLightAtPoint(left,lamps);

                    BaseTile t = Caster.GetMap(Floor(left.X / Caster.CellSize), Floor(left.Y / Caster.CellSize));
                    points[y*Buffer.Size.X+x] = new Vertex
                    {
                        Position = new Vector2f(x, y),
                        TexCoords = new Vector2f(t.FloorAtlas.X * Caster.CellSize + (left.X % Caster.CellSize), t.FloorAtlas.Y * Caster.CellSize + left.Y % Caster.CellSize),
                        Color = col
                    };

                    points[(Buffer.Size.Y - y) * Buffer.Size.X + x] = new Vertex
                    {
                        Position = new Vector2f(x, Buffer.Size.Y - y),
                        TexCoords = new Vector2f(t.CeilAtlas.X * Caster.CellSize + (left.X % Caster.CellSize), t.CeilAtlas.Y * Caster.CellSize + left.Y % Caster.CellSize),
                        Color = col
                    };

                    left += delta;
            }
        }

        public Renderer(RayCaster rc, RenderWindow w, RenderTexture rt, float fov)
        {
            Caster = rc;
            Screen = w;
            Buffer = rt;
            Fov = fov;
            HalfFov = fov / 2;
            DistanceToProjectionPlane = (rt.Size.X / 2) / TanD(fov / 2);
            Textures = new List<Texture>();
            DepthPerStrip = new float[Buffer.Size.X];
            AtlasTileSize = new Vector2f(Caster.CellSize, Caster.CellSize);
            AmbientLight = new Color(32, 32, 32);
            Angles = new float[Buffer.Size.X];
            LightMultiplier = 800;

            for (int x = 0; x < Buffer.Size.X; x++)
                Angles[x] = AtanD((x - Buffer.Size.X / 2.0f) / DistanceToProjectionPlane);

            floorVertices = new Vertex[Buffer.Size.X * Buffer.Size.Y];
            wallVertices = new Vertex[Buffer.Size.X*2];

            DisplayVertices = new Vertex[] {
                    new Vertex
                    {
                        Position = new Vector2f(0,0),
                        TexCoords = new Vector2f(0,Buffer.Size.Y-1),
                        Color = Color.White
                    },
                    new Vertex
                    {
                        Position = new Vector2f(0,Screen.Size.Y-1),
                        TexCoords = new Vector2f(0,0),
                        Color = Color.White
                    },
                    new Vertex
                    {
                        Position = new Vector2f(Screen.Size.X-1,Screen.Size.Y-1),
                        TexCoords = new Vector2f(Buffer.Size.X-1,0),
                        Color = Color.White
                    },
                    new Vertex
                    {
                        Position = new Vector2f(Screen.Size.X-1,0),
                        TexCoords = new Vector2f(Buffer.Size.X-1,Buffer.Size.Y-1),
                        Color = Color.White
                    },
                };
        }

        public void GenerateLightMap(List<Light> lamps)
        {
            if (!DynLight)
            {
                LightMap = new Color[Floor(Caster.CellSize * Caster.Map.GetLength(0) * LightMapScaler), Floor(Caster.CellSize * Caster.Map.GetLength(1) * LightMapScaler)];
                for (int y = 0; y < LightMap.GetLength(1); y++)
                    for (int x = 0; x < LightMap.GetLength(0); x++)
                        LightMap[x, y] = AmbientLight;

                for (int y = 0; y < LightMap.GetLength(1); y++)
                    for (int x = 0; x < LightMap.GetLength(0); x++)
                    {
                        float r = 0;
                        float g = 0;
                        float b = 0;
                        foreach (Light l in lamps)
                        {
                            float i = calculateLampIntensityAtPoint(new Vector2f(x / (float)LightMapScaler, y / (float)LightMapScaler), l.Position);
                            r += i * l.Color.R;
                            g += i * l.Color.G;
                            b += i * l.Color.B;
                        }
                        LightMap[x, y] += new Color(Clamp<byte>(r, 0, 255), Clamp<byte>(g, 0, 255), Clamp<byte>(b, 0, 255));

                        //LightMap[Clamp<int>(x - 1, 0, LightMap.GetLength(0) - 1), Clamp<int>(y, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));
                        //LightMap[Clamp<int>(x + 1, 0, LightMap.GetLength(0) - 1), Clamp<int>(y, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));
                        //LightMap[Clamp<int>(x, 0, LightMap.GetLength(0) - 1), Clamp<int>(y + 1, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));
                        //LightMap[Clamp<int>(x, 0, LightMap.GetLength(0) - 1), Clamp<int>(y - 1, 0, LightMap.GetLength(0) - 1)] += new Color(Clamp<byte>(r / 4, 0, 255), Clamp<byte>(g / 4, 0, 255), Clamp<byte>(b / 4, 0, 255));
                    }
            }
        }


        private Color getLightAtPoint(Vector2f point, List<Light> lamps)
        {
            float r = 0;
            float g = 0;
            float b = 0;
            Color ret = AmbientLight;
            foreach (Light l in lamps)
            {
                float i = calculateLampIntensityAtPoint(point, l.Position);
                r += i * l.Color.R;
                g += i * l.Color.G;
                b += i * l.Color.B;
            }
            ret += new Color(Clamp<byte>(r, 0, 255), Clamp<byte>(g, 0, 255), Clamp<byte>(b, 0, 255));

            return ret;
        }

        private Vertex[] floorVertices;
        private Vertex[] wallVertices;


        public void Render(Vector2f player, float angle, List<Sprite> sprites, List<Light> lamps)
        {
            List<Vertex> spritesLines = new List<Vertex>();

            //int MinLineHeight = (int)Buffer.Size.Y;
            int MinLineHeight = 1;
            ParallelLoopResult res = Parallel.For((int)Buffer.Size.Y / 2 + MinLineHeight, (int)Buffer.Size.Y, i => calculateFloor(player, angle, i, ref floorVertices, lamps));

            for (int x = 0; x < Buffer.Size.X; x++)
            {
                float rayAngle = Angles[x];
                RayResult ray = Caster.RayCast(player, angle + rayAngle);
                BaseTile t = Caster.GetMap(ray.Tile.X, ray.Tile.Y);

                if (!(t is CircleTile) | true) {
                    DepthPerStrip[x] = ray.Magnitude * CosD(rayAngle);
                    int lineHeightHalf = Floor((Caster.CellSize / DepthPerStrip[x]) * DistanceToProjectionPlane) >> 1;
                    if (lineHeightHalf < MinLineHeight)
                        MinLineHeight = lineHeightHalf;


                    (Vector2f textureCordUp, Vector2f textureCordDown) = t.CalculateTextureCoords(ray, player, angle, Caster);

                    Color col;

                    if (false) {
                        int pX = (int)(ray.Position.X * LightMapScaler);
                        if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0) - 1;
                        if (pX < 0) pX = 0;

                        int pY = (int)(ray.Position.Y * LightMapScaler);
                        if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                        if (pY < 0) pY = 0;
                        col = LightMap[pX, pY];
                    }
                    else
                        col = getLightAtPoint(ray.Position,lamps);

                    wallVertices[x << 1] = new Vertex
                    {
                        Position = new Vector2f(x, Buffer.Size.Y / 2 - lineHeightHalf),
                        Color = col,
                        TexCoords = textureCordUp
                    };
                    wallVertices[(x << 1) + 1] = new Vertex
                    {
                        Position = new Vector2f(x, Buffer.Size.Y / 2 + lineHeightHalf),
                        Color = col,
                        TexCoords = textureCordDown
                    };
                }
                else
                {
                    wallVertices[x << 1] = new Vertex();
                    wallVertices[(x << 1) + 1] = new Vertex();
                }
            }

            
            List<Sprite> finalList = new List<Sprite>();
            foreach (Sprite spr in sprites)
            {
                if (!DynLight)
                {
                    int pX = (int)(spr.Position.X * LightMapScaler);
                    if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0) - 1;
                    if (pX < 0) pX = 0;

                    int pY = (int)(spr.Position.Y * LightMapScaler);
                    if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                    if (pY < 0) pY = 0;

                    spr.Light = LightMap[pX, pY];
                }
                else
                    spr.Light = getLightAtPoint(spr.Position,lamps);

                spr.RenderPosition = Rotate(spr.Position - player, -angle);
                spr.Distance = spr.RenderPosition.X;
                if (spr.Distance > 0)
                    finalList.Add(spr);
            }

            finalList = finalList.OrderByDescending(s => s.RenderPosition.X).ToList();

            foreach (Sprite spr in finalList)
            {

                int lineHeight = (int)((Caster.CellSize / spr.Distance) * DistanceToProjectionPlane);
                int px = (int)(Buffer.Size.X / 2 + (spr.RenderPosition.Y / spr.Distance) * DistanceToProjectionPlane);
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

            while (!res.IsCompleted)
                Thread.Sleep(0);

            Buffer.Draw(floorVertices, 0, (uint)floorVertices.Length, PrimitiveType.Points, new RenderStates(Textures[MapAtlasInUse]));
            Buffer.Draw(wallVertices, 0, (uint)wallVertices.Length, PrimitiveType.Lines, new RenderStates(Textures[MapAtlasInUse]));
            Buffer.Draw(spritesLines.ToArray(), 0, (uint)spritesLines.Count, PrimitiveType.Lines, new RenderStates(Textures[MapAtlasInUse]));
        }

        public void ShowBuffer()
        {
            Screen.Draw(DisplayVertices, 0, 4, PrimitiveType.Quads, new RenderStates(Buffer.Texture));
            Screen.Display();
        }
    }
}
