﻿//using AOSharp.Core;
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

//            // SMovementController.Set(); // Alternatively you can just .Set(), which will load the default settings for everything and populate or overwrite the SMovementController.Instance
//        }

//        public static void Generation()
//        {
//            NavMeshGenerationSettings settings = NavMeshGenerationSettings.Default;
//            settings.AgentHeight = 1.7f; // Needs more testing
//            settings.AgentRadius = 0.25f; // Padding
//            settings.CellSize = 0.4f; // Don't go too low on this (especially on big zones)
//            settings.CellHeight = 0.07f; // Needs more testing
//            // settings.Bounds = new AOSharp.Common.GameData.Rect(0, 0, 100, 100) // If you need to specify certain generation bounds

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
//        }

//        public static void SaveNavMeshToFile(NavMesh navMesh)
//        {
//            SNavMeshSerializer.SaveToFile(navMesh, $"C:\\Users\\someuser\\Desktop\\navmeshtest\\test.nav");
//        }

//        public static void LoadNavMeshFromFile()
//        {
//            if (SNavMeshSerializer.LoadFromFile($"C:\\Users\\someuser\\Desktop\\navmeshtest\\test.nav", out NavMesh navMesh))
//            {

//            }
//        }

//        public static void GenerateAndSetPath()
//        {
//            List<Vector3> _testPath = SMovementController.Instance?.GeneratePath(DynelManager.LocalPlayer.Position, new Vector3(107.3, 3.3, 97.6));
//            SMovementController.Instance?.SetPath(_testPath); // Sets the path
//            // SMovementController.Instance.AppendPath(_testPath);
//        }
//    }
//}