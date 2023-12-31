﻿using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Collections.Generic;
using AOSharp.Core.UI;
using SharpNav;
using System;

namespace AOSharp.Pathfinding
{
    public class SNavMeshMovementController : SMovementController
    {
        public delegate NavMesh NavmeshResolveDelegate(SNavMeshMovementController movementController);
        public event NavmeshResolveDelegate NavmeshResolve;

        protected readonly string _navMeshFolderPath;

        private const float PathUpdateInterval = 1f;
        private double _nextPathUpdate = 0;
        private TriMeshData _triMeshData;
        private SNavMeshSettings _navMeshSettings;
        private NavMeshGenerationSettings _genSettings;
        private SPathfinder _pathFinder;

        public SNavMeshMovementController(SNavMeshControllerSettings settings) : base(settings.PathSettings)
        {
            _navMeshSettings = settings.NavMeshSettings;
        }

        public SNavMeshMovementController() : base() 
        {
            var controllerSettings = new SNavMeshControllerSettings();
            _navMeshSettings = controllerSettings.NavMeshSettings;
            _pathSettings = controllerSettings.PathSettings;
        }

        internal override void Update(object sender, float time)
        {
            if (_navMeshSettings.DrawNavMesh && _triMeshData != null)
            {
                _triMeshData.Draw(_navMeshSettings.DrawDistance);

                bool isOnNavMesh = _pathFinder.IsOnNavMesh(_genSettings.AgentRadius, out Vector3 hitPos);

                Vector3 color = isOnNavMesh ? DebuggingColor.Green : DebuggingColor.Red;
                SDebug.DrawCylinder(hitPos, _genSettings.AgentRadius, _genSettings.AgentHeight, color);
            }

            if (IsNavigating && _paths.Peek() is SNavMeshPath path && path.Initialized && Time.NormalTime > _nextPathUpdate)
            {
                path.UpdatePath();
                _nextPathUpdate = Time.NormalTime + PathUpdateInterval;
            }

            base.Update(sender,time);
        }

        /// <summary>
        /// Generates a path from given coordinates.
        /// </summary>
        public List<Vector3> GeneratePath(Vector3 startPos, Vector3 endPos) => _pathFinder.GeneratePath(startPos, endPos);

        /// <summary>
        /// Generates a path from given coordinate and returns the path distance.
        /// </summary>
        public bool GetDistance(Vector3 destination, out float distance)
        {
            distance = 0;

            try
            {
                List<Vector3> path = _pathFinder.GeneratePath(DynelManager.LocalPlayer.Position, destination);

                for(int i = 0; i < path.Count - 1; i++)
                    distance += Vector3.Distance(path[i], path[i+1]);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets NavMesh
        /// </summary>
        public void SetNavmesh(NavMesh navmesh)
        {
            if (navmesh == null)
                return;

            _pathFinder = new SPathfinder(navmesh);
            _genSettings = navmesh.Settings;

            if (_navMeshSettings.DrawNavMesh)
                _triMeshData = new TriMeshData(navmesh);
        }
    }

    public class SNavMeshPath : Path
    {
        private SPathfinder _pathfinder;
        public readonly Vector3 Destination;

        public SNavMeshPath(SPathfinder pathfinder, Vector3 dstPos) : base(new List<Vector3>())
        {
            _pathfinder = pathfinder;
            Destination = dstPos;
        }

        public override void OnPathStart()
        {
            UpdatePath();

            base.OnPathStart();
        }

        public override void OnPathFinished()
        {
            base.OnPathFinished();
        }

        internal void UpdatePath()
        {
            try
            {
                SetWaypoints(_pathfinder.GeneratePath(DynelManager.LocalPlayer.Position, Destination));
            }
            catch(Exception ex)
            {
                Chat.WriteLine(ex.Message);
                SetWaypoints(new List<Vector3>());
            }
        }
    }
}
