using System;
using AOSharp.Core.UI;
using SharpNav;
using SharpNav.IO.Json;

namespace AOSharp.Pathfinding
{
    public class SNavMeshSerializer
    {
        public bool SaveToFile(TiledNavMesh navMesh, string path)
        {
            if (navMesh == null)
            {
                Chat.WriteLine("No navmesh generated or loaded, cannot save.");
                return false;
            }

            try
            {
                new NavMeshJsonSerializer().Serialize(path, navMesh);
            }
            catch (Exception e)
            {
                Chat.WriteLine("Navmesh saving failed with exception:" + Environment.NewLine + e.ToString());
                return false;
            }


            Console.WriteLine("Saved to file!");
            return true;
        }

        public bool LoadFromFile(string path, out TiledNavMesh navMesh)
        {
            navMesh = null;

            try
            {
                navMesh = new NavMeshJsonSerializer().Deserialize(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Navmesh loading failed with exception:" + Environment.NewLine + e.ToString());
                return false;

            }

            return true;
        }
    }
}
