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

        public Renderer(RayCaster rc, RenderTexture rt, float fov)
        {
            Caster = rc;
            Buffer = rt;
            Fov = fov;
            DistanceToProjectionPlane = (rt.Size.X / 2) / TanD(fov / 2);
            Textures = new List<Texture>();
        }

        public void Render(Vector2f player, float angle)
        {
            List<Vertex> vertices = new List<Vertex>();

            for (int x = 0; x < Buffer.Size.X; x++)
            {
                float rayAngle = AtanD((x - Buffer.Size.X / 2.0f) / DistanceToProjectionPlane);
                RayResult ray = Caster.RayCast(player, angle + rayAngle);
                int lineHeightHalf = Floor((Caster.CellSize / (ray.Magnitude * CosD(rayAngle))) * DistanceToProjectionPlane) / 2;
                Vector2i textureCordUp;
                Vector2i textureCordDown;

                switch (ray.Side)
                {
                    case Side.Down:
                        break;
                    case Side.Up:
                        break;
                    case Side.Left:
                        break;
                    case Side.Right:
                        break;
                }

                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Floor(Buffer.Size.Y / 2 - lineHeightHalf)),
                    
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Floor(Buffer.Size.Y / 2 + lineHeightHalf)),
                    
                });
            }

            Buffer.Draw(vertices.ToArray(), 0, (uint)vertices.Count, PrimitiveType.Lines);
        }
    }
}
