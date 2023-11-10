//using AOSharp.Core;
//using SharpNav;
//using System.Collections.Generic;
//using Vector3 = AOSharp.Common.GameData.Vector3;
//using AOSharp.Pathfinding;

//namespace SharpNav
//{
//    public class Examples
//    {
//        public static void Generation()
//        {
//            NavMeshGenerationSettings settings = NavMeshGenerationSettings.Default;
//            settings.AgentHeight = 1.7f; // Needs more testing
//            settings.AgentRadius = 0.25f; // Padding
//            settings.CellSize = 0.4f; // Don't go too low on this (especially on big zones)
//            settings.CellHeight = 0.07f; // Needs more testing

//            SNavMeshGenerator.Generate(settings).ContinueWith(navMesh =>
//            {
//                if (navMesh.Result == null)
//                    return;

//                var navMeshControllerSettings = new SNavMeshControllerSettings
//                {
//                    NavMeshSettings = new SNavMeshSettings { DrawDistance = 100, DrawNavMesh = true },
//                    PathSettings = new SPathSettings { DrawPath = false }
//                };

//                SMovementController.Set(new SNavMeshMovementController(navMeshControllerSettings));
//                SMovementController.Instance.SetNavmesh(navMesh.Result);
//            });
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
//         // SMovementController.Instance.AppendPath(_testPath);
//        }
//    }
//}