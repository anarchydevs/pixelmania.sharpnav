using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Collections.Generic;
using org.critterai.nav;
using AOSharp.Core.UI;
using NavmeshMovementController;
using SharpNav;

namespace AOSharp.Pathfinding
{
    public class SNavMeshMovementController : SMovementController
    {
        private const float PathUpdateInterval = 1f;

        private SPathfinder _pathFinder;
        protected readonly string _navMeshFolderPath;
        private double _nextPathUpdate = 0;

        public delegate Navmesh NavmeshResolveDelegate(SNavMeshMovementController movementController);
        public event NavmeshResolveDelegate NavmeshResolve;
        private TriMeshData _triMeshData;
        private SNavMeshSettings _navMeshSettings;

        public SNavMeshMovementController(SNavMeshControllerSettings settings) : base(settings.PathSettings.DrawPath)
        {
            _navMeshSettings = settings.NavMeshSettings;
        }

        public SNavMeshMovementController() : base(false) 
        {
            _navMeshSettings = new SNavMeshSettings();
        }

        public override void Update(object sender, float time)
        {
            if (_navMeshSettings.DrawNavMesh && _triMeshData != null)
            {
                _triMeshData.Draw(_navMeshSettings.DrawDistance);
            }

            if (IsNavigating && _paths.Peek() is SNavMeshPath path && path.Initialized && Time.NormalTime > _nextPathUpdate)
            {
                path.UpdatePath();
                _nextPathUpdate = Time.NormalTime + PathUpdateInterval;
            }

            base.Update(sender,time);
        }

        public List<Vector3> GeneratePath(Vector3 startPos, Vector3 endPos) => _pathFinder.GeneratePath(startPos, endPos);

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
            catch (PointNotOnNavMeshException e)
            {
                return false;
            }
        }

        public void SetNavmesh(NavMesh navmesh)
        {
            if (navmesh == null)
                return;

            _pathFinder = new SPathfinder(navmesh);

            if (_navMeshSettings.DrawNavMesh)
                _triMeshData = new TriMeshData(navmesh);
        }

        public void SetNavmesh(TiledNavMesh navmesh)
        {
            if (navmesh == null)
                return;

            _pathFinder = new SPathfinder(navmesh);

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
                base.SetWaypoints(_pathfinder.GeneratePath(DynelManager.LocalPlayer.Position, Destination));
            }
            catch(PointNotOnNavMeshException e)
            {
                Chat.WriteLine(e.Message);
                base.SetWaypoints(new List<Vector3>());
            }
        }
    }
}
