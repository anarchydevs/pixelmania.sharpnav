using System;
using AOSharp.Common.GameData;
using AOSharp.Core.UI;
using SharpNav;
using SharpNav.IO.Json;

namespace AOSharp.Pathfinding
{
    public class SNavMeshSerializer
    {
        public static bool SaveToFile(NavMesh navMesh, string path)
        {
            if (navMesh == null)
            {
                Chat.WriteLine("No navmesh generated or loaded, cannot save.", ChatColor.Red);
                return false;
            }

            try
            {
                new NavMeshSerializer().Serialize(path, navMesh);
            }
            catch (Exception e)
            {
                Chat.WriteLine("Navmesh saving failed with exception:" + Environment.NewLine + e.ToString(), ChatColor.Red);
                return false;
            }

            Chat.WriteLine("Successfully saved NavMesh file.", ChatColor.Green);
            return true;
        }

        public static bool LoadFromFile(string path, out NavMesh navMeshBake)
        {
            navMeshBake = null;

            try
            {
                navMeshBake = new NavMeshSerializer().Deserialize(path);

                if (navMeshBake == null)
                    return false;
            }
            catch (Exception e)
            {
                Chat.WriteLine("Navmesh loading failed with exception:" + Environment.NewLine + e.ToString(), ChatColor.Red);
                return false;
            }

            Chat.WriteLine("Successfully loaded NavMesh file.", ChatColor.Green);
            return true;
        }
    }
}
