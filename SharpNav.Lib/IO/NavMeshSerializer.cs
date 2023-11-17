using System.Collections.Generic;
using System.IO;
using SharpNav.Collections;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using AOSharp.Common.GameData;
using System.Linq;
using AOSharp.Core.UI;
using static SharpNav.PolyMeshDetail;

namespace SharpNav.IO.Json
{
    /// <summary>
    /// Subclass of NavMeshSerializer that implements 
    /// serialization/deserializtion in text files with json format
    /// </summary>
    public class NavMeshSerializer
    {
        private static readonly string NAVMESH_VERSION = "1.0.0";

        public NavMeshSerializer()
        {
        }

        public void Serialize(string path, NavMesh mesh)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(NAVMESH_VERSION);
                WriteNavMeshGenerationSettings(writer,mesh.Settings);
                WriteVector3(writer, mesh.Origin);
                writer.Write(mesh.TileWidth);
                writer.Write(mesh.TileHeight);
                writer.Write(mesh.MaxTiles);
                writer.Write(mesh.MaxPolys);

                writer.Write(mesh.Tiles.Count());

                foreach (NavTile tile in mesh.Tiles)
                {
                    NavPolyId id = mesh.GetTileRef(tile);
                    WriteNavTile(writer, tile, id);
                }
            }
        }

        private void WriteNavMeshGenerationSettings(BinaryWriter writer, NavMeshGenerationSettings settings)
        {
            writer.Write(settings.CellSize);
            writer.Write(settings.CellHeight);
            writer.Write(settings.MaxClimb);
            writer.Write(settings.AgentHeight);
            writer.Write(settings.AgentRadius);
            writer.Write(settings.MinRegionSize);
            writer.Write(settings.MergedRegionSize);
            writer.Write(settings.MaxEdgeLength);
            writer.Write(settings.MaxEdgeError);
            writer.Write((byte)settings.ContourFlags);
            writer.Write(settings.VertsPerPoly);
            writer.Write(settings.SampleDistance);
            writer.Write(settings.MaxSampleError);
            writer.Write(settings.BuildBoundingVolumeTree);
            writer.Write(settings.Bounds.MinX);
            writer.Write(settings.Bounds.MinY);
            writer.Write(settings.Bounds.MaxX);
            writer.Write(settings.Bounds.MaxY);
        }

        private void WriteVector3(BinaryWriter writer, Geometry.Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        private void WritePolyVertex(BinaryWriter writer, PolyVertex polyVertex)
        {
            writer.Write(polyVertex.X);
            writer.Write(polyVertex.Y);
            writer.Write(polyVertex.Z);
        }

        private void WriteNavTile(BinaryWriter writer, NavTile tile, NavPolyId id)
        {
            writer.Write(id.Id);
            WriteVector2i(writer, tile.Location);
            writer.Write(tile.Layer);
            writer.Write(tile.Salt);
            WriteBounds(writer, tile.Bounds);
            writer.Write(tile.Polys.Count());

            foreach (NavPoly navPoly in tile.Polys)
                WriteNavPoly(writer, navPoly);

            writer.Write(tile.Verts.Count());

            foreach (Geometry.Vector3 vector in tile.Verts)
                WriteVector3(writer, vector);

            writer.Write(tile.DetailMeshes.Count());

            foreach (MeshData meshData in tile.DetailMeshes)
                WriteMeshData(writer, meshData);

            writer.Write(tile.DetailVerts.Count());

            foreach (Geometry.Vector3 detailVert in tile.DetailVerts)
                WriteVector3(writer, detailVert);

            writer.Write(tile.DetailTris.Count());

            foreach (TriangleData triData in tile.DetailTris)
                WriteTriangleData(writer, triData);

            writer.Write(tile.OffMeshConnections.Count());

            foreach (OffMeshConnection meshConnection in tile.OffMeshConnections)
                WriteOffMeshConnection(writer, meshConnection);

            writer.Write(tile.BVTree.Count);
            writer.Write(tile.BvQuantFactor);
            writer.Write(tile.WalkableClimb);

            for (int i = 0; i < tile.BVTree.Count; i++)
                WriteBVTreeNode(writer, tile.BVTree[i]);
        }

        private void WriteTriangleData(BinaryWriter writer, TriangleData triData)
        {
            writer.Write(triData.VertexHash0);
            writer.Write(triData.VertexHash1);
            writer.Write(triData.VertexHash2);
            writer.Write(triData.Flags);
        }

        private void WriteMeshData(BinaryWriter writer, MeshData meshData)
        {
            writer.Write(meshData.VertexIndex);
            writer.Write(meshData.VertexCount);
            writer.Write(meshData.TriangleIndex);
            writer.Write(meshData.TriangleCount);
        }

        private void WriteBounds(BinaryWriter writer, BBox3 bounds)
        {
            WriteVector3(writer, bounds.Min);
            WriteVector3(writer, bounds.Max);
        }

        private void WriteVector2i(BinaryWriter writer, Vector2i location)
        {
            writer.Write(location.X);
            writer.Write(location.Y);
        }

        private void WriteBVTreeNode(BinaryWriter writer, BVTree.Node node)
        {
            WritePolyVertex(writer, node.Bounds.Min);
            WritePolyVertex(writer, node.Bounds.Max);
            writer.Write(node.Index);
        }

        private void WriteOffMeshConnection(BinaryWriter writer, OffMeshConnection meshConnection)
        {
            WriteVector3(writer, meshConnection.Pos0);
            WriteVector3(writer, meshConnection.Pos1);
            writer.Write(meshConnection.Radius);
            writer.Write(meshConnection.Poly);
            writer.Write((byte)meshConnection.Flags);
            writer.Write((byte)meshConnection.Side);
            // writer.Write(meshConnection.Tag); not sure what this is, skipping for now
        }

        private void WriteNavPoly(BinaryWriter writer, NavPoly navPoly)
        {
            writer.Write((byte)navPoly.PolyType);

            writer.Write(navPoly.Links.Count());

            foreach (Link link in navPoly.Links)
            {
                WriteLink(writer, link);
            }

            writer.Write(navPoly.Verts.Count());

            foreach (var vert in navPoly.Verts)
                writer.Write(vert);

            writer.Write(navPoly.Neis.Count());


            foreach (var neis in navPoly.Neis)
                writer.Write(neis);

            // writer.Write(navPoly.Tag); Not sure what this is, skipping for now
            writer.Write(navPoly.VertCount);
            writer.Write(navPoly.Area.Id);
        }

        private void WriteLink(BinaryWriter writer, Link link)
        {
            writer.Write(link.Reference.Id);
            writer.Write(link.Edge);
            writer.Write((byte)link.Side);
            writer.Write(link.BMin);
            writer.Write(link.BMax);
        }

        public NavMesh Deserialize(string path)
        {
            NavMesh navMesh;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    string version = reader.ReadString();

                    if (version != NAVMESH_VERSION)
                    {
                        Chat.WriteLine("File is using an old nav mesh version. Loading is cancelled.", ChatColor.Red);
                        return null;
                    }

                    NavMeshGenerationSettings settings = ReadNavMeshGenerationSettings(reader);

                    navMesh = new NavMesh(ReadVector3(reader), reader.ReadSingle(), reader.ReadSingle(), reader.ReadInt32(), reader.ReadInt32());
                    navMesh.Settings = settings;

                    List<NavTile> tiles = new List<NavTile>();

                    int tileCount = reader.ReadInt32();

                    for (int i = 0; i < tileCount; i++)
                    {
                        NavTile navTile = ReadNavTile(reader, navMesh.IdManager, out NavPolyId tileRef);
                        navMesh.AddTileAt(navTile, tileRef);
                    }
                }
            }

            return navMesh;
        }

        private Geometry.Vector3 ReadVector3(BinaryReader reader)
        {
            Geometry.Vector3 vector3 = new Geometry.Vector3();

            vector3.X = reader.ReadSingle();
            vector3.Y = reader.ReadSingle();
            vector3.Z = reader.ReadSingle();

            return vector3;
        }
        private Geometry.Vector2i ReadVector2i(BinaryReader reader)
        {
            Geometry.Vector2i vector2i = new Geometry.Vector2i();

            vector2i.X = reader.ReadInt32();
            vector2i.Y = reader.ReadInt32();

            return vector2i;
        }


        private NavMeshGenerationSettings ReadNavMeshGenerationSettings(BinaryReader reader)
        {
            NavMeshGenerationSettings settings = new NavMeshGenerationSettings();
            settings.CellSize = reader.ReadSingle();
            settings.CellHeight = reader.ReadSingle();
            settings.MaxClimb = reader.ReadSingle();
            settings.AgentHeight = reader.ReadSingle();
            settings.AgentRadius = reader.ReadSingle();
            settings.MinRegionSize = reader.ReadInt32();
            settings.MergedRegionSize = reader.ReadInt32();
            settings.MaxEdgeLength = reader.ReadInt32();
            settings.MaxEdgeError = reader.ReadSingle();
            settings.ContourFlags = (ContourBuildFlags)reader.ReadByte();
            settings.VertsPerPoly = reader.ReadInt32();
            settings.SampleDistance = reader.ReadInt32();
            settings.MaxSampleError = reader.ReadInt32();
            settings.BuildBoundingVolumeTree = reader.ReadBoolean();
            Rect bounds = new Rect(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            settings.Bounds = bounds;

            return settings;
        }
        private NavTile ReadNavTile(BinaryReader reader, NavPolyIdManager idManager, out NavPolyId tileRef)
        {
            int id = reader.ReadInt32();
            tileRef = new NavPolyId(id);
            Vector2i location = ReadVector2i(reader);
            int layer = reader.ReadInt32();
            int salt = reader.ReadInt32();
            NavTile navTile = new NavTile(location, layer, idManager, tileRef);

            navTile.Salt = salt;
            navTile.Bounds = ReadBounds(reader);
            navTile.PolyCount = reader.ReadInt32();
            navTile.Polys = new NavPoly[navTile.PolyCount];
          
            for (int i = 0; i < navTile.PolyCount; i++)
                navTile.Polys[i] = ReadNavPoly(reader);

            navTile.Verts = new Geometry.Vector3[reader.ReadInt32()];

            for (int i = 0; i < navTile.Verts.Count(); i++)
                navTile.Verts[i] = ReadVector3(reader);

            navTile.DetailMeshes = new MeshData[reader.ReadInt32()];

            for (int i = 0; i < navTile.DetailMeshes.Count(); i++)
                navTile.DetailMeshes[i] = ReadDetailMesh(reader);

            navTile.DetailVerts = new Geometry.Vector3[reader.ReadInt32()];

            for (int i = 0; i < navTile.DetailVerts.Count(); i++)
                navTile.DetailVerts[i] = ReadVector3(reader);

            navTile.DetailTris = new TriangleData[reader.ReadInt32()];

            for (int i = 0; i < navTile.DetailTris.Count(); i++)
                navTile.DetailTris[i] = ReadTriangleData(reader);

            navTile.OffMeshConnectionCount = reader.ReadInt32();
            navTile.OffMeshConnections = new OffMeshConnection[navTile.OffMeshConnectionCount]; 

            for (int i = 0; i < navTile.OffMeshConnectionCount; i++)
                navTile.OffMeshConnections[i] = ReadOffMeshConnection(reader);

            navTile.BvNodeCount = reader.ReadInt32();
            navTile.BvQuantFactor = reader.ReadSingle();
            navTile.WalkableClimb = reader.ReadSingle();

            List<BVTree.Node> bvTreeNodes = new List<BVTree.Node>();

            for (int i = 0; i < navTile.BvNodeCount; i++)
                bvTreeNodes.Add(ReadBVTreeNode(reader));

            navTile.BVTree = new BVTree(bvTreeNodes);
            return navTile;
        }

        private BVTree.Node ReadBVTreeNode(BinaryReader reader)
        {
            BVTree.Node node = new BVTree.Node();
            
            node.Bounds = new PolyBounds(ReadPolyVertex(reader), ReadPolyVertex(reader));
            node.Index = reader.ReadInt32();

            return node;
        }

        private PolyVertex ReadPolyVertex(BinaryReader reader)
        {
            PolyVertex polyVertex = new PolyVertex();
          
            polyVertex.X = reader.ReadInt32();
            polyVertex.X = reader.ReadInt32();
            polyVertex.X = reader.ReadInt32();

            return polyVertex;
        }

        private OffMeshConnection ReadOffMeshConnection(BinaryReader reader)
        {
            OffMeshConnection offMeshConnection = new OffMeshConnection();

            offMeshConnection.Pos0 = ReadVector3(reader);
            offMeshConnection.Pos1 = ReadVector3( reader);
            offMeshConnection.Radius = reader.ReadSingle();
            offMeshConnection.Poly = reader.ReadInt32();
            offMeshConnection.Flags = (OffMeshConnectionFlags)reader.ReadByte();
            offMeshConnection.Side = (BoundarySide)reader.ReadByte();

            return offMeshConnection;
        }

        private TriangleData ReadTriangleData(BinaryReader reader)
        {
            TriangleData triData = new TriangleData();
          
            triData.VertexHash0 = reader.ReadInt32();
            triData.VertexHash1 = reader.ReadInt32();
            triData.VertexHash2 = reader.ReadInt32();
            triData.Flags = reader.ReadInt32();

            return triData;
        }

        private MeshData ReadDetailMesh(BinaryReader reader)
        {
            MeshData meshData = new MeshData();

            meshData.VertexIndex = reader.ReadInt32();
            meshData.VertexCount = reader.ReadInt32();
            meshData.TriangleIndex = reader.ReadInt32();
            meshData.TriangleCount = reader.ReadInt32();

            return meshData;
        }

        private NavPoly ReadNavPoly(BinaryReader reader)
        {
            NavPoly navPoly = new NavPoly();

            navPoly.PolyType = (NavPolyType)reader.ReadByte();
            
            var linkCount = reader.ReadInt32();

            for (int i = 0; i < linkCount; i++)
                navPoly.Links.Add(ReadLink(reader));

            navPoly.Verts = new int[reader.ReadInt32()];

            for (int i = 0; i < navPoly.Verts.Count(); i++)
                navPoly.Verts[i] = reader.ReadInt32();

            navPoly.Neis = new int[reader.ReadInt32()];

            for (int i = 0; i < navPoly.Neis.Count(); i++)
                navPoly.Neis[i] = reader.ReadInt32();

            navPoly.VertCount = reader.ReadInt32();
            navPoly.Area = new Area(reader.ReadByte());

            return navPoly;
        }

        private Link ReadLink(BinaryReader reader)
        {
            Link link = new Link();
            link.Reference = new NavPolyId(reader.ReadInt32());
            link.Edge = reader.ReadInt32();
            link.Side = (BoundarySide)reader.ReadByte();
            link.BMin = reader.ReadInt32();
            link.BMax = reader.ReadInt32();

            return link;
        }

        private BBox3 ReadBounds(BinaryReader reader)
        {
            BBox3 bounds = new BBox3(ReadVector3(reader), ReadVector3(reader));

            return bounds;
        }
    }
}
