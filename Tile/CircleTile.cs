using SFML.System;
using System;
using static SFMLTest.Helpers;
using static SFMLTest.RayCaster;
using static MathFloat.MathF;
using MathFloat;

namespace SFMLTest.Tile
{
    class CircleTile : BaseTile
    {

        public override RayResult OnIntersection(RayResult original, Vector2f O, float angle, RayCaster caster)
        {
            int points = LineCircleIntersections(caster.CellSize * original.Tile.X + (caster.CellSize / 2),
                                                 caster.CellSize * original.Tile.Y + (caster.CellSize / 2),
                                                 caster.CellSize / 2,
                                                 original.Position,
                                                 original.Position + new Vector2f(300 * CosD(angle), 300 * SinD(angle)),
                                                 out Vector2f p1, out Vector2f p2);

            switch (points)
            {
                case 0:
                    original.Valid = false;
                    return original;
                case 1:
                    return new RayResult
                    {
                        Valid = true,
                        Magnitude = Distance(O, p1),
                        Side = Side.None,
                        Position = p1,
                        Tile = original.Tile
                    };
                case 2:
                    Vector2f final = Distance(O, p1) <= Distance(O, p2) ? p1 : p2;
                    return new RayResult
                    {
                        Valid = true,
                        Magnitude = Distance(O, final),
                        Side = Side.None,
                        Position = final,
                        Tile = original.Tile
                    };
            }

            original.Valid = true;
            return original;
        }

        public override (Vector2f top, Vector2f bottom) CalculateTextureCoords(RayResult original, Vector2f O, float angle, RayCaster caster)
        {
            float tx = Atan2(caster.CellSize * original.Tile.Y + (caster.CellSize / 2) - original.Position.Y,
                             caster.CellSize * original.Tile.X + (caster.CellSize / 2) - original.Position.X) + (float)Math.PI;

            tx = (float)((tx * caster.CellSize) / (Math.PI * 2));
            tx *= 4;
            tx %= (float)(Math.PI * 2);
            Vector2f top = new Vector2f(
                        DownAtlas.X * caster.CellSize + tx,
                        DownAtlas.Y * caster.CellSize);
            Vector2f bottom = new Vector2f(
                top.X,
                top.Y + caster.CellSize);

            return (top, bottom);
        }
    }
}
