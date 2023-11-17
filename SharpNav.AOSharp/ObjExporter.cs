using SharpNav.Geometry;
using System.Collections.Generic;
using System;
using System.IO;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using AOSharp.Common.GameData;
using System.Diagnostics;
using System.Threading.Tasks;
using AOSharp.Core;
using System.Text;

public static class ObjExporter
{
    public static void SaveCurrentPlayfield(string folderPath,bool mergeTriangles = true)
    {
        long prevMs = 0;
        Stopwatch sw = Stopwatch.StartNew();

        Task.Run(() =>
        {
            var triData = TerrainData.GetTriGeometry(Rect.Default, sw, ref prevMs);

            if (mergeTriangles)
                ExportAndMergeTrianglesToObj(triData, sw, ref prevMs, $"{folderPath}\\{Playfield.TilemapResourceId}.obj");
            else
                ExportTrianglesToObj(triData, sw, ref prevMs, $"{folderPath}\\{Playfield.TilemapResourceId}.obj");
        });
    }

    private static void ExportAndMergeTrianglesToObj(IEnumerable<Triangle3> triangles, Stopwatch sw, ref long prevMs, string filePath)
    {
        Chat.WriteLine($"Starting triangle to obj conversion.", ChatColor.Green);
        prevMs = sw.ElapsedMilliseconds;

        Dictionary<SharpNav.Geometry.Vector3, int> vertexIndices = new Dictionary<SharpNav.Geometry.Vector3, int>();
        List<SharpNav.Geometry.Vector3> vertices = new List<SharpNav.Geometry.Vector3>();
        List<int> indices = new List<int>();

        int index = 1;

        foreach (var triangle in triangles)
        {
            ProcessVertex(triangle.A, vertexIndices, vertices, indices, ref index);
            ProcessVertex(triangle.B, vertexIndices, vertices, indices, ref index);
            ProcessVertex(triangle.C, vertexIndices, vertices, indices, ref index);
        }

        Chat.WriteLine($"Merged triangles. {(sw.ElapsedMilliseconds - prevMs).FormatTime()}", ChatColor.Green);
        prevMs = sw.ElapsedMilliseconds;

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var vertex in vertices)
                    writer.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");


                Chat.WriteLine($"Wrote vertices to obj file. {(sw.ElapsedMilliseconds - prevMs).FormatTime()}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;

                for (int i = 0; i < indices.Count; i += 3)
                    writer.WriteLine($"f {indices[i]} {indices[i + 1]} {indices[i + 2]}");


                Chat.WriteLine($"Wrote indices to obj file. {(sw.ElapsedMilliseconds - prevMs).FormatTime()}", ChatColor.Green);
            }

            Chat.WriteLine($"Total time: {sw.ElapsedMilliseconds.FormatTime()}", ChatColor.Green);
            Chat.WriteLine("OBJ file exported successfully.", ChatColor.Green);
        }
        catch (Exception ex)
        {
            Chat.WriteLine($"Error exporting OBJ file: {ex.Message}", ChatColor.Red);
        }
    }

    private static void ProcessVertex(SharpNav.Geometry.Vector3 vertex, Dictionary<SharpNav.Geometry.Vector3, int> vertexIndices, List<SharpNav.Geometry.Vector3> vertices, List<int> indices, ref int index)
    {
        if (!vertexIndices.ContainsKey(vertex))
        {
            vertexIndices[vertex] = index;
            vertices.Add(vertex);
            indices.Add(index);
            index++;
        }
        else
        {
            indices.Add(vertexIndices[vertex]);
        }
    }

    private static void ExportTrianglesToObj(IEnumerable<Triangle3> triangles, Stopwatch sw, ref long prevMs, string filePath)
    {
        Chat.WriteLine($"Starting triangle to obj conversion.", ChatColor.Green);

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var triangle in triangles)
                {
                    writer.WriteLine($"v {triangle.A.X} {triangle.A.Y} {triangle.A.Z}");
                    writer.WriteLine($"v {triangle.B.X} {triangle.B.Y} {triangle.B.Z}");
                    writer.WriteLine($"v {triangle.C.X} {triangle.C.Y} {triangle.C.Z}");
                }


                Chat.WriteLine($"Wrote vertices to obj file. {(sw.ElapsedMilliseconds - prevMs).FormatTime()}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;

                int index = 1;

                foreach (var _ in triangles)
                    writer.WriteLine($"f {index++} {index++} {index++}");

                Chat.WriteLine($"Wrote indices to obj file. {(sw.ElapsedMilliseconds - prevMs).FormatTime()}", ChatColor.Green);
                prevMs = sw.ElapsedMilliseconds;

            }

            Chat.WriteLine($"Total time: {sw.ElapsedMilliseconds.FormatTime()}", ChatColor.Green);
            Chat.WriteLine("OBJ file exported successfully.", ChatColor.Green);
        }
        catch (Exception ex)
        {
            Chat.WriteLine($"Error exporting OBJ file: {ex.Message}", ChatColor.Red);
        }
    }
}