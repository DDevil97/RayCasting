using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SFMLTest.RayCaster;
using Sprite = SFMLTest.RayCaster.Sprite;

namespace SFMLTest
{
    class Renderer
    {
        private float HalfFov { get; set; }
        private float DistanceToProjectionPlane { get; set; }
        private float[] Angles { get; set; }
        private float[] DepthPerStrip { get; set; }
        private float[,] LightMap{get;set;}

        public float Fov { get; set; }
        public RayCaster Caster { get; set; }
        public RenderTexture Buffer { get; set; }
        public List<Texture> Textures { get; set; }
        public Vector2f AtlasTileSize { get; set; }

        public int MapAtlasInUse { get; set; }

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
            Angles = new float[Buffer.Size.X];

            for (int x = 0; x < Buffer.Size.X; x++)
                Angles[x] = AtanD((x - Buffer.Size.X / 2.0f) / DistanceToProjectionPlane);
        }

        public void GenerateLightMap(List<Vector3f> lamps)
        {
            LightMap = new float[Caster.CellSize * Caster.Map.GetLength(0)*4, Caster.CellSize * Caster.Map.GetLength(1)*4];

            for (int y = 0; y < LightMap.GetLength(1); y++)
                for (int x = 0;x < LightMap.GetLength(0); x++)
                {
                    foreach (Vector3f l in lamps)
                    {
                        LightMap[x,y] += lampIntensityAtPoint(new Vector2f(x/4f,y/4f), new Vector2f(l.X, l.Y));
                    }
                    LightMap[x, y] += 0.1f;
                    if (LightMap[x, y] >= 1) LightMap[x, y] = 1;
                }
        }

        private float lampIntensityAtPoint(Vector2f mPos, Vector2f lPos)
        {
            RayResult r = Caster.RayCast(lPos, Atan2D(mPos.Y - lPos.Y, mPos.X - lPos.X));
            if (r.Magnitude + 1 >= Distance(mPos, lPos))
            {
                return 1f / (float)Math.Sqrt(Distance(mPos, lPos)) * 4;
            }
            else
                return 0;
        }

        public void Render(Vector2f player, float angle, List<Sprite> sprites, List<Vector3f> lamps)
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

                int pX = Floor(ray.Position.X*4);
                if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0)-1;
                if (pX <0) pX = 0;

                int pY = Floor(ray.Position.Y*4);
                if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                if (pY < 0) pY = 0;

                float i = LightMap[pX , pY];

                Color wc = new Color((byte)(i * 255), (byte)(i * 255), (byte)(i * 255));

                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Buffer.Size.Y / 2 - lineHeightHalf),
                    Color = wc,
                    TexCoords = textureCordUp
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Buffer.Size.Y / 2 + lineHeightHalf),
                    Color = wc,
                    TexCoords = textureCordDown
                });
            }

            for (int y = Floor(Buffer.Size.Y / 2) + MinLineHeight; y < Buffer.Size.Y; y++)
            {
                float dist = ((Caster.CellSize / (y - Buffer.Size.Y / 2f)) * DistanceToProjectionPlane / 2f) / CosD(HalfFov);
                Vector2f left = player + new Vector2f(dist * CosD(angle - HalfFov), dist * SinD(angle - HalfFov));
                Vector2f right = player + new Vector2f(dist * CosD(angle + HalfFov), dist * SinD(angle + HalfFov));
                Vector2f delta = (right - left) / Buffer.Size.X;

                for (int x = 0; x < Buffer.Size.X; x++)
                {

                    int pX = Floor(left.X*4);
                    if (pX >= LightMap.GetLength(0)) pX = LightMap.GetLength(0) - 1;
                    if (pX < 0) pX = 0;

                    int pY = Floor(left.Y*4);
                    if (pY >= LightMap.GetLength(0)) pY = LightMap.GetLength(1) - 1;
                    if (pY < 0) pY = 0;

                    float i = LightMap[pX, pY];
                    Color wc = new Color((byte)(i * 255), (byte)(i * 255), (byte)(i * 255));
                    TileInfo t = Caster.GetMap(Floor(left.X / Caster.CellSize), Floor(left.Y / Caster.CellSize));
                    points.Add(new Vertex
                    {
                        Position = new Vector2f(x, y),
                        TexCoords = new Vector2f(t.FloorAtlas.X * Caster.CellSize + (left.X % Caster.CellSize), t.FloorAtlas.Y * Caster.CellSize + left.Y % Caster.CellSize),
                        Color = wc
                    });

                    points.Add(new Vertex
                    {
                        Position = new Vector2f(x, Buffer.Size.Y - y),
                        TexCoords = new Vector2f(t.CeilAtlas.X * Caster.CellSize + (left.X % Caster.CellSize), t.CeilAtlas.Y * Caster.CellSize + left.Y % Caster.CellSize),
                        Color = wc
                    });

                    left += delta;
                }
            };

            List<Sprite> finalList = new List<Sprite>();
            foreach (Sprite spr in sprites)
            {
                spr.Position = RotateAround(spr.Position, player, -angle);
                spr.Distance = spr.Position.X - player.X;
                if (spr.Distance > 0)
                    finalList.Add(spr);
            }

            finalList = finalList.OrderByDescending(s => s.Position.X - player.X).ToList();

            foreach (Sprite spr in finalList)
            {
                int lineHeight = Floor((Caster.CellSize / spr.Distance) * DistanceToProjectionPlane);
                int px = (int)Buffer.Size.X / 2 + Floor(((spr.Position.Y - player.Y) / spr.Distance) * DistanceToProjectionPlane);

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
                            Position = new Vector2f(posX, Buffer.Size.Y / 2 - lineHeight / 2),
                            Color = Color.White,
                            TexCoords = textureCordUp
                        });
                        spritesLines.Add(new Vertex
                        {
                            Position = new Vector2f(posX, Buffer.Size.Y / 2 + lineHeight / 2),
                            Color = Color.White,
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
