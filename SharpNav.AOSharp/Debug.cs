using AOSharp.Common.GameData;
using AOSharp.Core;
using System;

namespace AOSharp.Pathfinding
{
    internal class SDebug
    {
        internal static void DrawCylinder(Vector3 bottomPosition, float radius, float height, Vector3 color, float segments = 16)
        {
            float angleStep = 360f / segments;

            Vector3 startPoint = Vector3.Zero;
            Vector3 endPoint = Vector3.Zero;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep;
                float nextAngle = (i + 1) * angleStep;

                startPoint.X = bottomPosition.X + radius * (float)Math.Cos(angle * Math.PI / 180);
                startPoint.Y = bottomPosition.Y;
                startPoint.Z = bottomPosition.Z + radius * (float)Math.Sin(angle * Math.PI / 180);

                endPoint.X = bottomPosition.X + radius * (float)Math.Cos(nextAngle * Math.PI / 180);
                endPoint.Y = bottomPosition.Y;
                endPoint.Z = bottomPosition.Z + radius * (float)Math.Sin(nextAngle * Math.PI / 180);

                Debug.DrawLine(startPoint, endPoint, color);
                Debug.DrawLine(startPoint + new Vector3(0, height, 0), endPoint + new Vector3(0, height, 0), color);
                Debug.DrawLine(startPoint, startPoint + new Vector3(0, height, 0), color);
            }
        }
    }
}