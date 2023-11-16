using System;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Common.GameData;
using sVector3 = SharpNav.Geometry.Vector3;
using NavmeshQuery = SharpNav.NavMeshQuery;
using PathCorridor = SharpNav.Crowds.PathCorridor;
using SharpNav.Pathfinding;
using SharpNav;
using AOSharp.Core.UI;
using AOSharp.Core;

namespace AOSharp.Pathfinding
{
    public class SPathfinder
    {
        private TiledNavMesh _navMesh;
        private NavmeshQuery _query;
        private NavQueryFilter _filter;
        private PathCorridor _pathCorridor;

        public SPathfinder(TiledNavMesh navMesh)
        {
            _navMesh = navMesh;
            _filter = new NavQueryFilter();
            _query = new NavmeshQuery(navMesh, 2048);
            _pathCorridor = new PathCorridor();
        }

        internal bool IsOnNavMesh(float radius, out Vector3 hitPos)
        {
            Vector3 rayOrigin = DynelManager.LocalPlayer.Position;
            Vector3 rayTarget = DynelManager.LocalPlayer.Position;
            rayTarget.Y = 0;

            if (!Playfield.Raycast(rayOrigin, rayTarget, out hitPos, out _))
                hitPos = rayOrigin;

            sVector3 startPos = hitPos.ToSharpNav();
            sVector3 extents = new sVector3(5f, 5f, 5f);

            try
            {
                _query.FindNearestPoly(ref startPos, ref extents, out NavPoint pt);
                startPos.Y = 0;
                pt.Position.Y = 0;

                return Vector3.Distance(startPos.ToVector3(), pt.Position.ToVector3()) < radius;
            }
            catch
            {
                return false;
            }
        }

        internal List<Vector3> GeneratePath(Vector3 start, Vector3 end)
        {
            List<Vector3> finalPath = new List<Vector3>();


            if (!FindNearestPoint(start, new sVector3(0.5f, 2, 0.5f), out NavPoint origin) || origin.Position == new sVector3())
            {
                Chat.WriteLine("Could not find valid origin point on navmesh");
                return new List<Vector3>();
            }

            if (!FindNearestPoint(end, new sVector3(0.5f, 2, 0.5f), out NavPoint destination) || destination.Position == new sVector3())
            {
                Chat.WriteLine("Could not find valid destination point on navmesh");
                return new List<Vector3>();
            }

            SharpNav.Pathfinding.Path path = new SharpNav.Pathfinding.Path();

            if (!_query.FindPath(ref origin, ref destination, _filter, path))
                return new List<Vector3>();


           // if(!FindSmoothPath(path, origin, destination, out List<sVector3> smoothPath))
            //    return new List<Vector3>();

             sVector3[] straightPath = StraightenPath(start.ToSharpNav(), end.ToSharpNav(), path);

            finalPath.AddRange(straightPath.Select(node => new Vector3(node.X, node.Y, node.Z)));

            return finalPath;
        }

        private bool FindSmoothPath(SharpNav.Pathfinding.Path path, NavPoint origin, NavPoint destination, out List<sVector3> smoothPath)
        {
            smoothPath = new List<sVector3>(2048);

            //find a smooth path over the mesh surface
            int npolys = path.Count;
            sVector3 iterPos = new sVector3();
            sVector3 targetPos = new sVector3();
            _query.ClosestPointOnPoly(origin.Polygon, origin.Position, ref iterPos);
            _query.ClosestPointOnPoly(path[npolys - 1], destination.Position, ref targetPos);

            smoothPath.Add(iterPos);

            float STEP_SIZE = 0.5f;
            float SLOP = 0.01f;

            while (npolys > 0 && smoothPath.Count < smoothPath.Capacity)
            {
                //find location to steer towards
                sVector3 steerPos = new sVector3();
                StraightPathFlags steerPosFlag = 0;
                NavPolyId steerPosRef = NavPolyId.Null;

                if (!GetSteerTarget(_query, iterPos, targetPos, SLOP, path, ref steerPos, ref steerPosFlag, ref steerPosRef))
                    break;

                bool endOfPath = (steerPosFlag & StraightPathFlags.End) != 0 ? true : false;
                bool offMeshConnection = (steerPosFlag & StraightPathFlags.OffMeshConnection) != 0 ? true : false;

                //find movement delta
                sVector3 delta = steerPos - iterPos;
                float len = (float)Math.Sqrt(sVector3.Dot(delta, delta));

                //if steer target is at end of path or off-mesh link
                //don't move past location
                if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                    len = 1;
                else
                    len = STEP_SIZE / len;

                sVector3 moveTgt = new sVector3();
                VMad(ref moveTgt, iterPos, delta, len);

                //move
                sVector3 result = new sVector3();
                List<NavPolyId> visited = new List<NavPolyId>(16);
                NavPoint startPoint = new NavPoint(path[0], iterPos);
                _query.MoveAlongSurface(ref startPoint, ref moveTgt, out result, visited);
                path.FixupCorridor(visited);
                npolys = path.Count;
                float h = 0;
                _query.GetPolyHeight(path[0], result, ref h);
                result.Y = h;
                iterPos = result;

                //handle end of path when close enough
                if (endOfPath && InRange(iterPos, steerPos, SLOP, 1.0f))
                {
                    //reached end of path
                    iterPos = targetPos;
                    if (smoothPath.Count < smoothPath.Capacity)
                    {
                        smoothPath.Add(iterPos);
                    }
                    break;
                }

                //store results
                if (smoothPath.Count < smoothPath.Capacity)
                {
                    smoothPath.Add(iterPos);
                }
            }
            
            return true;
        }

        private bool GetSteerTarget(NavMeshQuery navMeshQuery, sVector3 startPos, sVector3 endPos, float minTargetDist, SharpNav.Pathfinding.Path path,
            ref sVector3 steerPos, ref StraightPathFlags steerPosFlag, ref NavPolyId steerPosRef)
        {
            StraightPath steerPath = new StraightPath();
            navMeshQuery.FindStraightPath(startPos, endPos, path, steerPath, 0);
            int nsteerPath = steerPath.Count;
            if (nsteerPath == 0)
                return false;

            //find vertex far enough to steer to
            int ns = 0;
            while (ns < nsteerPath)
            {
                if ((steerPath[ns].Flags & StraightPathFlags.OffMeshConnection) != 0 ||
                    !InRange(steerPath[ns].Point.Position, startPos, minTargetDist, 1000.0f))
                    break;

                ns++;
            }

            //failed to find good point to steer to
            if (ns >= nsteerPath)
                return false;

            steerPos = steerPath[ns].Point.Position;
            steerPos.Y = startPos.Y;
            steerPosFlag = steerPath[ns].Flags;
            if (steerPosFlag == StraightPathFlags.None && ns == (nsteerPath - 1))
                steerPosFlag = StraightPathFlags.End; // otherwise seeks path infinitely!!!
            steerPosRef = steerPath[ns].Point.Polygon;

            return true;
        }

        private bool InRange(sVector3 v1, sVector3 v2, float r, float h)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;
            return (dx * dx + dz * dz) < (r * r) && Math.Abs(dy) < h;
        }

        private void VMad(ref sVector3 dest, sVector3 v1, sVector3 v2, float s)
        {
            dest.X = v1.X + v2.X * s;
            dest.Y = v1.Y + v2.Y * s;
            dest.Z = v1.Z + v2.Z * s;
        }

        //private PathCorridor GeneratePathCorridor(Vector3 start, Vector3 end)
        //{
        //    if (!FindNearestPoint(start, new sVector3(0.3f, 2, 0.3f), out NavPoint origin) || origin.Position == new sVector3())
        //    {
        //        Chat.WriteLine("Could not find nearest origin point");
        //        return null;
        //    }
        //    if (!FindNearestPoint(end, new sVector3(0.3f, 2, 0.3f), out NavPoint destination) || destination.Position == new sVector3())
        //    {
        //        Chat.WriteLine("Could not find nearest destination point");
        //        return null;
        //    }

        //    SharpNav.Pathfinding.Path path = new SharpNav.Pathfinding.Path();

        //    if (!_query.FindPath(ref origin, ref destination, _filter, path))
        //    {
        //        Chat.WriteLine("Could not find path");
        //        return null;
        //    }

        //    _pathCorridor.SetCorridor(end.ToSharpNav(), path);
        //    _pathCorridor.MovePosition(destination.Position, _query);

        //    return _pathCorridor;
        //}

        private sVector3[] StraightenPath(sVector3 start, sVector3 end, SharpNav.Pathfinding.Path path)
        {
            StraightPath straightPath = new StraightPath();

            if (!_query.FindStraightPath(start, end, path, straightPath, PathBuildFlags.None))
                throw new Exception("Failed to straighten path.");

            List<sVector3> paths = new List<sVector3>();

            for (int i = 0; i < straightPath.Count; i++)
                paths.Add(straightPath[i].Point.Position);

            return paths.ToArray();
        }

        public bool FindNearestPoint(Vector3 position, sVector3 extents, out NavPoint point)
        {
            point = new NavPoint();

            try
            {
                sVector3 startPoint = position.ToSharpNav();
                _query.FindNearestPoly(ref startPoint, ref extents, out point);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public bool IsUsingNavmesh(TiledNavMesh navmesh)
        {
            return navmesh == _navMesh;
        }
    }
}