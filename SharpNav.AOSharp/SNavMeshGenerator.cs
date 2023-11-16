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
using System.Collections.Concurrent;
using SharpNav.Geometry;

namespace AOSharp.Pathfinding
{
    public class SNavMeshGenerator
    {
        public async static Task<NavMesh> GenerateAsync(NavMeshGenerationSettings settings)
        {
            try
            {
                long prevMs = 0;
                Stopwatch sw = Stopwatch.StartNew();

                var navMeshBake = await Task.Run(() => Generate(GetTriGeometry(settings.Bounds, sw, ref prevMs), settings));

                Chat.WriteLine($"Generated nav mesh. {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);
                Chat.WriteLine($"Total generation time: {FormatTime(sw.ElapsedMilliseconds)}", ChatColor.Green);

                return navMeshBake;
            }
            catch (Exception ex)
            {
                Chat.WriteLine(ex.Message, ChatColor.Red);

                return null;
            }
        }

        public static bool Generate(NavMeshGenerationSettings settings, out NavMesh navMesh)
        {
            navMesh = null;

            try
            {
                long prevMs = 0;
                Stopwatch sw = Stopwatch.StartNew();
                List<STriangle3> triMesh = GetTriGeometry(settings.Bounds, sw, ref prevMs);

                navMesh = Generate(triMesh, settings);

                Chat.WriteLine($"Generated nav mesh. {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);
                Chat.WriteLine($"Total generation time: {FormatTime(sw.ElapsedMilliseconds)}", ChatColor.Green);

                return true;
            }
            catch (Exception ex)
            {
                Chat.WriteLine(ex.Message, ChatColor.Red);

                return false;
            }
        }

        private static List<STriangle3> GetTriGeometry(Rect bounds,Stopwatch sw, ref long prevMs)
        {
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

            Rect defaultBounds = Rect.Default;

            if (!(bounds.MinX == Rect.Default.MinX && bounds.MaxX == defaultBounds.MaxX && bounds.MinY == defaultBounds.MinY && bounds.MaxY == defaultBounds.MaxY))
            {
                ConcurrentBag<STriangle3> trianglesToRemove = new ConcurrentBag<STriangle3>();

                Parallel.ForEach(tris.ToList(), tri =>
                {
                    if (!bounds.Contains(tri.A.ToVector3()) ||
                        !bounds.Contains(tri.B.ToVector3()) ||
                        !bounds.Contains(tri.C.ToVector3()))
                    {
                        trianglesToRemove.Add(tri);
                    }
                });

                foreach (var triToRemove in trianglesToRemove)
                {
                    tris.Remove(triToRemove);
                }


                Chat.WriteLine($"Removing triangles outside of given bounds {bounds}. {FormatTime(sw.ElapsedMilliseconds - prevMs)}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;
            }

            return tris;
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

        /// <summary>
		/// Generates a <see cref="NavMesh"/> given a collection of triangles and some settings.
		/// </summary>
		/// <param name="triangles">The triangles that form the level.</param>
		/// <param name="settings">The settings to generate with.</param>
		/// <returns>A <see cref="NavMesh"/>.</returns>
		private static NavMesh Generate(IEnumerable<Triangle3> triangles, NavMeshGenerationSettings settings)
        {
            BBox3 bounds = triangles.GetBoundingBox(settings.CellSize);
            var hf = new Heightfield(bounds, settings);
            hf.RasterizeTriangles(triangles);
            hf.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            hf.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
            hf.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

            var chf = new CompactHeightfield(hf, settings);
            chf.Erode(settings.VoxelAgentRadius);
            chf.BuildDistanceField();
            chf.BuildRegions(2, settings.MinRegionSize, settings.MergedRegionSize);

            var cont = chf.BuildContourSet(settings);
            var polyMesh = new PolyMesh(cont, settings);
            var polyMeshDetail = new PolyMeshDetail(polyMesh, chf, settings);
            var buildData = new NavMeshBuilder(polyMesh, polyMeshDetail, new SharpNav.Pathfinding.OffMeshConnection[0], settings);
            var navMesh = new NavMesh(buildData);
            navMesh.Settings = settings;

            return navMesh;
        }
    }
}
