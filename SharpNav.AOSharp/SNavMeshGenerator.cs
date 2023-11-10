using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Linq;
using System.Collections.Generic;
using System;
using AOSharp.Core.UI;
using SharpNav;
using AOSharp.Common.Unmanaged.DbObjects;
using System.Diagnostics;
using Mesh = AOSharp.Common.GameData.Mesh;
using STriangle3 = SharpNav.Geometry.Triangle3;
using Vector3 = AOSharp.Common.GameData.Vector3;
using System.Threading.Tasks;

namespace AOSharp.Pathfinding
{
    public class SNavMeshGenerator
    {
        public async static Task<NavMesh> Generate(NavMeshGenerationSettings settings)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                long prevMs = 0;

                Chat.WriteLine("Starting Navmesh Generation", ChatColor.Green);

                List<Mesh> meshes = Playfield.IsDungeon ?
                                        DungeonTerrain.CreateFromCurrentPlayfield() :
                                        Terrain.CreateFromCurrentPlayfield();

                Chat.WriteLine($"Loaded terrain mesh data. {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;

                List<SurfaceResource> surfaces = Playfield.IsDungeon ?
                                        Playfield.Rooms.Select(x => x.SurfaceResource).ToList() :
                                        Playfield.Zones.Select(x => SurfaceResource.Get(Playfield.ModelIdentity.Instance << 16 | x.Instance)).ToList();

                Chat.WriteLine($"Loaded surface resource mesh data {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;

                List<STriangle3> tris = new List<STriangle3>();

                tris.AddRange(GetTriMesh(meshes));

                Chat.WriteLine($"Converted terrain mesh data to triangle data. {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;

                tris.AddRange(GetTriMesh(surfaces));

                Chat.WriteLine($"Converted surface resource mesh data to triangle data. {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;

                var navMesh = await Task.Run(() => NavMesh.Generate(tris, settings));

                Chat.WriteLine($"Generated nav mesh. {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);

                return navMesh;
            }
            catch (Exception ex)
            {
                Chat.WriteLine(ex.Message, ChatColor.Red);

                return null;
            }
        }

        private static string FormatTime(long miliseconds) => string.Format("{0:mm\\:ss\\.fff}", TimeSpan.FromMilliseconds(miliseconds));

        private static List<STriangle3> GetTriMesh(List<SurfaceResource> surfaces)
        {
            List<STriangle3> tris = new List<STriangle3>();

            foreach (var surface in surfaces)
            {
                if (surface == null)
                    continue;

                tris.AddRange(GetTriMesh(surface.Meshes));
            }

            return tris;
        }

        private static List<STriangle3> GetTriMesh(List<Mesh> meshes)
        {
            List<STriangle3> tris = new List<STriangle3>();

            foreach (var mesh in meshes)
            {
                for (int i = 0; i < mesh.Triangles.Count / 3; i++)
                {
                    int num = i * 3;
                    int index = mesh.Triangles[num];
                    int index2 = mesh.Triangles[num + 1];
                    int index3 = mesh.Triangles[num + 2];

                    Vector3[] array = new Vector3[3]
                    {
                        mesh.LocalToWorldMatrix.MultiplyPoint3x4(mesh.Vertices[index]),
                        mesh.LocalToWorldMatrix.MultiplyPoint3x4(mesh.Vertices[index2]),
                        mesh.LocalToWorldMatrix.MultiplyPoint3x4(mesh.Vertices[index3])
                    };

                    tris.Add(new STriangle3(array[0].ToSharpNav(), array[1].ToSharpNav(), array[2].ToSharpNav()));
                }
            }

            return tris;
        }
    }
}
