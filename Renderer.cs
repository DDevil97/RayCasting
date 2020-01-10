using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SFMLTest.RayCaster;

namespace SFMLTest
{
    class Renderer
    {
        public float Fov { get; set; }
        public float DistanceToProjectionPlane { get; set; }
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
            DistanceToProjectionPlane = (rt.Size.X / 2) / TanD(fov / 2);
            Textures = new List<Texture>();
            MapAtlasInUse = 0;
            AtlasTileSize = new Vector2f(Caster.CellSize,Caster.CellSize);
        }

        public void Render(Vector2f player, float angle)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<Vertex> points = new List<Vertex>();

            for (int x = 0; x < Buffer.Size.X; x++)
            {
                float rayAngle = AtanD((x - Buffer.Size.X / 2.0f) / DistanceToProjectionPlane);
                RayResult ray = Caster.RayCast(player, angle + rayAngle);
                int lineHeightHalf = Floor(((Caster.CellSize / (ray.Magnitude * CosD(rayAngle))) * DistanceToProjectionPlane) / 2);
                Vector2f textureCordUp;
                Vector2f textureCordDown;
                Vector2f floorCord;
                TileInfo t = Caster.GetMap(ray.Tile.X, ray.Tile.Y);
                

                switch (ray.Side)
                {
                    case Side.Down:
                        textureCordUp = new Vector2f(
                            t.DownAtlas.X * Caster.CellSize+(ray.Position.X % Caster.CellSize),
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
                            t.LeftAtlas.Y * Caster.CellSize );
                        textureCordDown = new Vector2f(
                            textureCordUp.X,
                            textureCordUp.Y + Caster.CellSize);
                        break;
                    case Side.Right:
                        textureCordUp = new Vector2f(
                            t.RightAtlas.X * Caster.CellSize + (ray.Position.Y % Caster.CellSize),
                            t.RightAtlas.Y * Caster.CellSize );
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

                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Floor(Buffer.Size.Y / 2 - lineHeightHalf)),
                    Color = Color.White,
                    TexCoords = textureCordUp
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Floor(Buffer.Size.Y / 2 + lineHeightHalf)),
                    Color = Color.White,
                    TexCoords = textureCordDown
                });

                for (int y = Floor(Buffer.Size.Y / 2 + lineHeightHalf); y < Buffer.Size.Y; y++)
                {
                    float DistanceToFloor = ((Caster.CellSize  / (y - Buffer.Size.Y/2f)) * DistanceToProjectionPlane/2) / CosD(rayAngle);
                    Vector2f floorPos = player + new Vector2f(DistanceToFloor * CosD(rayAngle + angle), DistanceToFloor * SinD(rayAngle + angle));
                    floorCord = new Vector2f(Caster.CellSize*2 + (floorPos.X % Caster.CellSize), floorPos.Y % Caster.CellSize);

                    points.Add(new Vertex
                    {
                        Position = new Vector2f(x,y),
                        TexCoords = floorCord,
                        Color = Color.White
                    });


                    points.Add(new Vertex
                    {
                        Position = new Vector2f(x, Buffer.Size.Y-y),
                        TexCoords = floorCord,
                        Color = Color.White
                    });
                }
            }

            Buffer.Draw(vertices.ToArray(), 0, (uint)vertices.Count, PrimitiveType.Lines,new RenderStates(Textures[MapAtlasInUse]));

            Buffer.Draw(points.ToArray(), 0, (uint)points.Count, PrimitiveType.Points, new RenderStates(Textures[MapAtlasInUse]));
        }
    }
}
