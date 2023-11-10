using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Linq;
using System.Collections.Generic;
using System;
using Vector3 = AOSharp.Common.GameData.Vector3;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using AOSharp.Core.UI;
using SharpNav;
using AOSharp.Common.Unmanaged.Imports;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Collections.Concurrent;
using SharpNav.Pathfinding;

namespace AOSharp.Pathfinding
{
    public class SMovementController
    {
        private const float UpdateInterval = 0.2f;
        private const float UnstuckInterval = 5f;
        private const float UnstuckThreshold = 2f;

        public bool IsNavigating => _paths.Count != 0;

        private double _nextUpdate = 0;
        private double _nextUstuckCheck = Time.NormalTime;
        private float _lastDist = 0f;
        private bool _drawPath;
        protected Queue<Path> _paths = new Queue<Path>();

        private ConcurrentQueue<MovementAction> _movementActionQueue = new ConcurrentQueue<MovementAction>();

        public EventHandler DestinationReached;

        public static SNavMeshMovementController Instance { get; internal set; }

        public SMovementController(bool drawPath = true)
        {
            _drawPath = drawPath;
        }

        public static void Set(SNavMeshMovementController movementcontroller)
        {
            Instance = movementcontroller;
        }

        public virtual void Update(object sender, float time)
        {
            try
            {
                while (_movementActionQueue.TryDequeue(out MovementAction action))
                    ChangeMovement(action);

                if (_paths.Count == 0)
                    return;

                if (!DynelManager.LocalPlayer.IsMoving)
                    SetMovement(MovementAction.ForwardStart);

                if (!_paths.Peek().Initialized)
                    _paths.Peek().OnPathStart();

                Vector3 waypoint;

                if (!_paths.Peek().GetNextWaypoint(out waypoint))
                {
                    Path path = _paths.Dequeue();
                    path.OnPathFinished();

                    if (_paths.Count == 0 && path.StopAtDest)
                    {
                        SetMovement(MovementAction.ForwardStop);
                        DestinationReached?.Invoke(null, null);
                    }

                    return;
                }

                if (Time.NormalTime > _nextUstuckCheck)
                {
                    float currentDist = DynelManager.LocalPlayer.Position.DistanceFrom(waypoint);

                    if (_lastDist - currentDist <= UnstuckThreshold)
                        OnStuck();

                    _lastDist = currentDist;
                    _nextUstuckCheck = Time.NormalTime + UnstuckInterval;
                }

                LookAt(waypoint);

                if (Time.NormalTime > _nextUpdate)
                {
                    SetMovement(MovementAction.Update);
                    _nextUpdate = Time.NormalTime + UpdateInterval;
                }

                if (_drawPath)
                    _paths.Peek().Draw();
            }
            catch (Exception e)
            {
                Chat.WriteLine($"This shouldn't happen pls report (MovementController): {e.Message}");
            }
        }

        public void Halt()
        {
            _paths.Clear();

            SetMovement(MovementAction.FullStop);
        }

        public void Follow(Identity identity)
        {
            Network.Send(new FollowTargetMessage
            {
                Type = FollowTargetType.Target,
                Info = new FollowTargetMessage.TargetInfo
                {
                    Target = identity
                }
            });
        }

        public void SetPath(List<Vector3> waypoints)
        {
            if (waypoints.Count() == 0 || waypoints == null)
                return;

            SetPath(waypoints, out _);
        }

        public void AppendPath(List<Vector3> waypoints)
        {
            if (waypoints.Count() == 0 || waypoints == null)
                return;

            AppendPath(waypoints, out _);
        }

        private bool SetPath(List<Vector3> waypoints, out Path path)
        {
            _paths.Clear();
            return AppendPath(waypoints, out path);
        }

        private bool AppendPath(List<Vector3> waypoints, out Path path)
        {
            path = new Path(waypoints);
            AppendPath(path);
            return true;
        }

        internal void AppendPath(Path path)
        {
            _paths.Enqueue(path);
        }

        public void LookAt(Vector3 pos)
        {
            Vector3 myPos = DynelManager.LocalPlayer.Position;
            myPos.Y = 0;
            Vector3 dstPos = pos;
            dstPos.Y = 0;
            DynelManager.LocalPlayer.Rotation = Quaternion.FromTo(myPos, dstPos);
        }

        protected virtual void OnStuck()
        {
            //Chat.WriteLine("Stuck!?");
        }

        //Must be called from game loop!
        private static void ChangeMovement(MovementAction action)
        {
            if (action == MovementAction.LeaveSit)
            {
                Network.Send(new CharacterActionMessage()
                {
                    Action = CharacterActionType.StandUp
                });
            }
            else
            {
                IntPtr pEngine = N3Engine_t.GetInstance();

                if (pEngine == IntPtr.Zero)
                    return;

                N3EngineClientAnarchy_t.MovementChanged(pEngine, action, 0, 0, true);
            }
        }

        public void SetMovement(MovementAction action)
        {
            _movementActionQueue.Enqueue(action);
        }
    }

    public class Path
    {
        public float NodeReachedDist = 0.05f;
        public float DestinationReachedDist = 0.05f;
        public bool StopAtDest = true;
        public bool Initialized = false;
        protected Queue<Vector3> _waypoints = new Queue<Vector3>();
        public Action DestinationReachedCallback;

        public Path(Vector3 destination) : this(new List<Vector3>() { destination })
        {

        }

        public Path(List<Vector3> waypoints)
        {
            SetWaypoints(waypoints);
        }

        public virtual void OnPathStart()
        {
            Initialized = true;
        }

        public virtual void OnPathFinished()
        {
            DestinationReachedCallback?.Invoke();
        }


        protected void SetWaypoints(List<Vector3> waypoints)
        {
            _waypoints.Clear();

            foreach (Vector3 waypoint in waypoints)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(waypoint) > NodeReachedDist)
                    _waypoints.Enqueue(waypoint);
            }
        }

        internal bool GetNextWaypoint(out Vector3 waypoint)
        {
            if (_waypoints.Count == 0)
            {
                waypoint = Vector3.Zero;
                return false;
            }

            Vector3 playerPos = DynelManager.LocalPlayer.Position;
            playerPos.Y = 0f;

            Vector3 targetPos = _waypoints.Peek();
            targetPos.Y = 0f;

            if (playerPos.DistanceFrom(targetPos) <= (_waypoints.Count > 1 ? NodeReachedDist : DestinationReachedDist))
            {
                _waypoints.Dequeue();

                if (_waypoints.Count == 0)
                {
                    waypoint = Vector3.Zero;
                    return false;
                }
            }

            waypoint = _waypoints.Peek();
            return true;
        }

        internal void Draw()
        {
            Vector3 lastWaypoint = DynelManager.LocalPlayer.Position;
            foreach (Vector3 waypoint in _waypoints)
            {
                Debug.DrawLine(lastWaypoint, waypoint, DebuggingColor.Yellow);
                lastWaypoint = waypoint;
            }
        }
    }

    public struct Tri
    {
        public Vector3 Vert1;
        public Vector3 Vert2;
        public Vector3 Vert3;

        private Vector3 _median;

        internal Tri(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vert1 = v1;
            Vert2 = v2;
            Vert3 = v3;

            _median = (Vert1 + Vert2 + Vert3) / 3;
        }

        internal void Draw(int drawDistance)
        {
            if (_median.DistanceFrom(DynelManager.LocalPlayer.Position) > drawDistance)
                return;

            Debug.DrawLine(Vert1, Vert2, DebuggingColor.Green);
            Debug.DrawLine(Vert2, Vert3, DebuggingColor.Green);
            Debug.DrawLine(Vert3, Vert1, DebuggingColor.Green);
        }
    }

    public class TriMeshData
    {
        internal List<Tri> _polyData;

        public TriMeshData(TiledNavMesh navMesh)
        {
            _polyData = new List<Tri>();

            int numTiles = navMesh.TileCount;

            for (int i = 0; i < numTiles; i++)
            {
                NavTile meshTile = navMesh[i];
                var verts = meshTile.Verts;

                foreach (var poly in meshTile.Polys)
                {
                    List<Vector3> vectors = new List<Vector3>();

                    _polyData.Add(new Tri(
                        verts[poly.Verts[0]].ToVector3(),
                        verts[poly.Verts[1]].ToVector3(),
                        verts[poly.Verts[2]].ToVector3()));
                }
            }
        }

        internal void Draw(int drawDistance = 100)
        {
            foreach (var poly in _polyData)
                poly.Draw(drawDistance);
        }
    }
}
