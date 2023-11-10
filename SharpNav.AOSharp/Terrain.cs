

using AOSharp.Common.GameData;
using AOSharp.Common.Unmanaged.DbObjects;
using AOSharp.Core;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AOSharp.Pathfinding
{
    public class DungeonTerrain
    {
        public static List<Mesh> CreateFromCurrentPlayfield()
        {
            if (!Playfield.IsDungeon)
            {
                throw new Exception("Must be in a dungeon!");
            }

            List<Mesh> list = new List<Mesh>();
            DungeonRDBTilemap tilemap = Playfield.RDBTilemap as DungeonRDBTilemap;
            foreach (Room room in Playfield.Rooms)
            {
                list.Add(CreateMesh(room, tilemap));
            }

            return list;
        }

        private static Mesh CreateMesh(Room room, DungeonRDBTilemap tilemap)
        {
            int num = (int)room.LocalRect.MaxX - (int)room.LocalRect.MinX;
            int num2 = (int)room.LocalRect.MaxY - (int)room.LocalRect.MinY;
            float num3 = (float)num * tilemap.TileSize;
            float num4 = (float)num2 * tilemap.TileSize;
            int num5 = num + 1;
            int num6 = num2 + 1;
            Vector3[] array = new Vector3[num5 * num6];
            List<int> list = new List<int>();
            Vector3 vector = new Vector3(1f, 0f, 1f);
            vector.X += (room.Center.X - (float)num / 2f) * tilemap.TileSize;
            vector.Z += (room.Center.Z - (float)num2 / 2f) * tilemap.TileSize;
            int num7 = 0;
            for (int i = 0; i < num6; i++)
            {
                for (int j = 0; j < num5; j++)
                {
                    byte b = tilemap.Heightmap[j + (int)room.LocalRect.MinX - 1, i + (int)room.LocalRect.MinY - 1];
                    Vector3 vector2 = default(Vector3);
                    vector2.X = (float)j * tilemap.TileSize - num3 / 2f;
                    vector2.Y = (float)(int)b * tilemap.HeightmapScale;
                    vector2.Z = (float)i * tilemap.TileSize - num4 / 2f;
                    Vector3 vector3 = vector2;
                    vector3.X -= vector.X;
                    vector3.Z -= vector.Z;
                    array[num7] = vector3;
                    num7++;
                }
            }

            num7 = 0;
            for (int k = 0; k < num2; k++)
            {
                for (int l = 0; l < num; l++)
                {
                    byte b2 = tilemap.CollisionData[l + (int)room.LocalRect.MinX, k + (int)room.LocalRect.MinY];
                    if (b2 > 0 && b2 != 128)
                    {
                        list.Add(k * num5 + l);
                        list.Add((k + 1) * num5 + l);
                        list.Add(k * num5 + l + 1);
                        list.Add((k + 1) * num5 + l);
                        list.Add((k + 1) * num5 + l + 1);
                        list.Add(k * num5 + l + 1);
                    }

                    num7 += 6;
                }
            }

            List<Vector3> list2 = array.ToList();
            List<int> list3 = list;
            int num8 = 0;
            while (num8 < list2.Count)
            {
                if (list3.Contains(num8))
                {
                    num8++;
                    continue;
                }

                list2.RemoveAt(num8);
                for (int m = 0; m < list3.Count; m++)
                {
                    if (list3[m] > num8)
                    {
                        list3[m]--;
                    }
                }
            }

            return new Mesh
            {
                Triangles = list3,
                Vertices = list2,
                Position = room.Position - new Vector3(0f, room.YOffset, 0f),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (double)room.Rotation * (Math.PI / 180.0)),
                Scale = new Vector3(1f, 1f, 1f)
            };
        }

    }


    public class Terrain
    {
        public static List<Mesh> CreateFromCurrentPlayfield()
        {
            if (Playfield.IsDungeon)
            {
                throw new Exception("Must be outdoors!");
            }

            List<Mesh> list = new List<Mesh>();
            OutdoorRDBTilemap outdoorRDBTilemap = Playfield.RDBTilemap as OutdoorRDBTilemap;
            foreach (OutdoorRDBTilemap.Chunk chunk in outdoorRDBTilemap.Chunks)
            {
                list.Add(CreateMesh(chunk, outdoorRDBTilemap.TileSize, outdoorRDBTilemap.HeightmapScale));
            }

            return list;
        }

        public static List<Mesh> CreateFromTilemap(OutdoorRDBTilemap tilemap)
        {
            List<Mesh> list = new List<Mesh>();
            foreach (OutdoorRDBTilemap.Chunk chunk in tilemap.Chunks)
            {
                list.Add(CreateMesh(chunk, tilemap.TileSize, tilemap.HeightmapScale));
            }

            return list;
        }

        private static Mesh CreateMesh(OutdoorRDBTilemap.Chunk chunk, float tileSize, float heightMapScale)
        {
            int num = 0;
            int num2 = 0;
            Vector3[] array = new Vector3[chunk.Size * chunk.Size];
            for (int i = 0; i < chunk.Size; i++)
            {
                for (int j = 0; j < chunk.Size; j++)
                {
                    array[num] = new Vector3((float)j * tileSize, (float)(int)chunk.Heightmap[j, i] * heightMapScale, (float)i * tileSize);
                    num++;
                }
            }

            List<int> list = new List<int>();
            for (int k = 0; k < chunk.Size - 1; k++)
            {
                for (int l = 0; l < chunk.Size - 1; l++)
                {
                    list.Add(k * chunk.Size + l + 1);
                    list.Add(k * chunk.Size + l);
                    list.Add((k + 1) * chunk.Size + l);
                    list.Add(k * chunk.Size + l + 1);
                    list.Add((k + 1) * chunk.Size + l);
                    list.Add((k + 1) * chunk.Size + l + 1);
                    num2 += 6;
                }
            }

            return new Mesh
            {
                Triangles = list,
                Vertices = array.ToList(),
                Position = new Vector3((float)(chunk.X * (chunk.Size - 1)) * tileSize, 0f, (float)(chunk.Y * (chunk.Size - 1)) * tileSize),
                Rotation = Quaternion.Identity,
                Scale = new Vector3(1f, 1f, 1f)
            };
        }
    }
}