using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Linq;
using System.Collections.Generic;
using System;
using AOSharp.Core.UI;
using SharpNav;
using System.Diagnostics;
using STriangle3 = SharpNav.Geometry.Triangle3;
using System.Threading.Tasks;
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

                var navMeshBake = await Task.Run(() => Generate(TerrainData.GetTriGeometry(settings.Bounds, sw, ref prevMs), settings));

                Chat.WriteLine($"Converted triangle data to navmesh. {(sw.ElapsedMilliseconds - prevMs).FormatTime()}", ChatColor.Green);
                Chat.WriteLine($"Total generation time: {sw.ElapsedMilliseconds.FormatTime()}", ChatColor.Green);

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
                List<STriangle3> triMesh = TerrainData.GetTriGeometry(settings.Bounds, sw, ref prevMs);

                navMesh = Generate(triMesh, settings);

                Chat.WriteLine($"Converted triangle data to navmesh. {(sw.ElapsedMilliseconds - prevMs).FormatTime()}", ChatColor.Green);
                Chat.WriteLine($"Total generation time: {sw.ElapsedMilliseconds.FormatTime()}", ChatColor.Green);

                return true;
            }
            catch (Exception ex)
            {
                Chat.WriteLine(ex.Message, ChatColor.Red);

                return false;
            }
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
            Chat.WriteLine("Loaded bounds");

            var hf = new Heightfield(bounds, settings);
            Chat.WriteLine("Init height field");

            hf.RasterizeTriangles(triangles);
            Chat.WriteLine("Rasterized triangles");

            hf.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            hf.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
            hf.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

            Chat.WriteLine("Loaded Heightfield");

            var chf = new CompactHeightfield(hf, settings);
            chf.Erode(settings.VoxelAgentRadius);
            chf.BuildDistanceField();
            chf.BuildRegions(2, settings.MinRegionSize, settings.MergedRegionSize);

            Chat.WriteLine("Loaded CompactHeightfield");

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