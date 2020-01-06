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

        public Renderer(RayCaster rc, RenderTexture rt, float fov)
        {
            Caster = rc;
            Buffer = rt;
            Fov = fov;
            DistanceToProjectionPlane = (rt.Size.X / 2) / TanD(fov / 2);
        }

        public void Render(Vector2f player, float angle)
        {
            List<Vertex> vertices = new List<Vertex>();

            for (int x = 0; x < Buffer.Size.X; x++)
            {
                float rayAngle = AtanD((x - Buffer.Size.X / 2.0f) / DistanceToProjectionPlane);
                RayResult ray = Caster.RayCast(player, angle + rayAngle);
                int lineHeightHalf = Round((Caster.CellSize / (ray.Magnitude * CosD(rayAngle))) * DistanceToProjectionPlane) / 2;
                Color color = Color.Black;
                switch (ray.Side)
                {
                    case Side.Down:
                    case Side.Up:
                        color = new Color(255, 255, 255, 255);
                        break;
                    case Side.Left:
                    case Side.Right:
                        color = new Color(128, 128, 128, 255);
                        break;
                }

                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Round(Buffer.Size.Y / 2 - lineHeightHalf)),
                    Color = color
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x, Round(Buffer.Size.Y / 2 + lineHeightHalf)),
                    Color = color
                });
            }

            Buffer.Draw(vertices.ToArray(), 0, (uint)vertices.Count, PrimitiveType.Lines);
        }
    }
}
