// Copyright (c) 2014 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
// Licensed under the MIT License - https://raw.github.com/Robmaister/SharpNav/master/LICENSE

using System;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;

namespace SharpNav
{
	//TODO right now this is basically an alias for TiledNavMesh. Fix this in the future.

	/// <summary>
	/// A TiledNavMesh generated from a collection of triangles and some settings
	/// </summary>
	public class NavMesh : TiledNavMesh
	{

		internal NavMeshGenerationSettings Settings;
        /// <summary>
        /// Initializes a new instance of the <see cref="NavMesh" /> class.
        /// </summary>
        /// <param name="builder">The NavMeshBuilder data</param>
        public NavMesh(NavMeshBuilder builder)
			: base(builder)
        {
        }

        public NavMesh(Vector3 origin, float tileWidth, float tileHeight, int maxTiles, int maxPolys)
           : base(origin, tileWidth, tileHeight, maxTiles, maxPolys)
        {
        }
	}
}


public class NavMeshBake
{
	public NavMeshGenerationSettings Settings { get; }
    public TiledNavMesh NavMesh { get; }

    public NavMeshBake(NavMeshGenerationSettings settings, TiledNavMesh navMesh)
	{
		Settings = settings;
		NavMesh = navMesh;	
    }
}