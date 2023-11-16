//using AOSharp.Core;
//using SharpNav;
//using System.Collections.Generic;
//using Vector3 = AOSharp.Common.GameData.Vector3;
//using AOSharp.Pathfinding;
//using AOSharp.Core.UI;

//namespace SharpNav
//{
//    public class Examples
//    {
//        public static void ControllerSetter()
//        {
//            var navMeshControllerSettings = new SNavMeshControllerSettings(new SNavMeshSettings(true, 100), new SPathSettings(true));
//            SMovementController.Set(new SNavMeshMovementController(navMeshControllerSettings));

//            //SMovementController.Set(); Alternatively you can just .Set(), which will load the default settings for everything and populate or overwrite the SMovementController.Instance
//        }

//        public static void Generation()
//        {
//            NavMeshGenerationSettings settings = NavMeshGenerationSettings.Default;
//            settings.AgentHeight = 1.7f; // Needs more testing
//            settings.AgentRadius = 0.25f; // Padding
//            settings.CellSize = 0.4f; // Don't go too low on this (especially on big zones)
//            settings.CellHeight = 0.07f; // Needs more testing

//            SNavMeshGenerator.GenerateAsync(settings).ContinueWith(navMesh =>
//            {
//                if (navMesh.Result == null)
//                    return;

//                SMovementController.Instance?.SetNavmesh(navMesh.Result);
//            });

//            //if (SNavMeshGenerator.Generate(settings, out NavMesh navMesh))  // Generation on main thread
//            //{
//            //    SMovementController.Instance?.SetNavmesh(navMesh);
//            //}

//            //You can also specify bounds as the second arguement if you only need a geometry section
//        }

//        public static void SaveNavMeshToFile(NavMesh navMesh)
//        {
//            SNavMeshSerializer serializer = new SNavMeshSerializer();

//            if (serializer.SaveToFile(navMesh, $"C:\\Users\\someuser\\Desktop\\navmeshtest\\test.nav"))
//            {

//            }
//        }

//        public static void LoadNavMeshFromFile()
//        {
//            SNavMeshSerializer serializer = new SNavMeshSerializer();

//            if (serializer.LoadFromFile($"C:\\Users\\someuser\\Desktop\\navmeshtest\\test.nav", out TiledNavMesh navMesh))
//            {

//            }
//        }

//        public static void GenerateAndSetPath()
//        {
//            List<Vector3> _testPath = SMovementController.Instance?.GeneratePath(DynelManager.LocalPlayer.Position, new Vector3(107.3, 3.3, 97.6));
//            SMovementController.Instance?.SetPath(_testPath);
//            // SMovementController.Instance.AppendPath(_testPath);
//        }
//    }
//}