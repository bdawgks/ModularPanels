using System.Numerics;

namespace ModularPanels.PanelLib
{
    public static class MathLib
    {
        public static Vector2 RotateVector(Vector2 v, float angle)
        {
            float angleRads = angle * MathF.PI / 180.0f;
            float sA = MathF.Sin(angleRads);
            float cA = MathF.Cos(angleRads);

            return new Vector2(v.X * cA - v.Y * sA, v.X * sA + v.Y * cA);
        }
    }
}
