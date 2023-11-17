using System;

namespace AOSharp.Pathfinding
{
    public static class Extensions
    {
        public static SharpNav.Geometry.Vector3 ToSharpNav(this AOSharp.Common.GameData.Vector3 vector3) => new SharpNav.Geometry.Vector3(vector3.X, vector3.Y, vector3.Z);

        public static Common.GameData.Vector3 ToVector3(this SharpNav.Geometry.Vector3 vector3) => new AOSharp.Common.GameData.Vector3(vector3.X, vector3.Y, vector3.Z);

        public static string FormatTime(this long miliseconds) => string.Format("{0:mm\\:ss\\.fff}", TimeSpan.FromMilliseconds(miliseconds));
    }
}