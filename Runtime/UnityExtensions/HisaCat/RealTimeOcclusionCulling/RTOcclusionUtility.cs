using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static class RTOcclusionUtility
    {
        /// <summary>
        /// Calculates the combined axis-aligned bounding box (AABB) of the given <paramref name="renderers"/>
        /// expressed in the local coordinate space of <paramref name="referenceTransform"/>.
        /// Each renderer's local bounds are transformed to world space and then into the reference's local space
        /// (via all 8 corner points) to properly account for rotation and non-uniform scale before encapsulation.
        /// </summary>
        /// <param name="referenceTransform">Transform that defines the local space the result is expressed in.</param>
        /// <param name="renderers">Renderers to include when computing the combined bounds.</param>
        /// <returns>
        /// Combined AABB in <paramref name="referenceTransform"/> local space, or default if no valid renderers.
        /// </returns>
        public static Bounds CalculateRenderersCombinedLocalBounds(Transform referenceTransform, List<Renderer> renderers)
        {
            if (renderers == null) return default;
            if (renderers.Count == 0) return default;

            // Filter only active-in-hierarchy renderers to ignore hidden/inactive ones.
            var activeRenderers = new List<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.gameObject.activeInHierarchy)
                    activeRenderers.Add(renderer);
            }

            if (activeRenderers.Count == 0) return default;

            // Compute AABB in the reference transform's local space.
            // For each renderer:
            //   renderer-local (bounds corners) -> world -> reference-local, then encapsulate.
            var worldToLocal = referenceTransform.worldToLocalMatrix;
            bool initialized = false;
            Bounds combined = default;
            {
                int count = activeRenderers.Count;
                for (int i = 0; i < count; i++)
                {
                    var renderer = activeRenderers[i];
                    var localBounds = renderer.localBounds;
                    var (c, e) = (localBounds.center, localBounds.extents);

                    const int cornerCount = 8;
                    var localCorners = new Vector3[cornerCount]
                    {
                        new (c.x - e.x, c.y - e.y, c.z - e.z), // 0: -X, -Y, -Z
                        new (c.x + e.x, c.y - e.y, c.z - e.z), // 1: +X, -Y, -Z
                        new (c.x - e.x, c.y + e.y, c.z - e.z), // 2: -X, +Y, -Z
                        new (c.x + e.x, c.y + e.y, c.z - e.z), // 3: +X, +Y, -Z
                        new (c.x - e.x, c.y - e.y, c.z + e.z), // 4: -X, -Y, +Z
                        new (c.x + e.x, c.y - e.y, c.z + e.z), // 5: +X, -Y, +Z
                        new (c.x - e.x, c.y + e.y, c.z + e.z), // 6: -X, +Y, +Z
                        new (c.x + e.x, c.y + e.y, c.z + e.z)  // 7: +X, +Y, +Z
                    };

                    // Matrix to convert renderer-local points into world space.
                    var localToWorld = renderer.localToWorldMatrix;
                    for (int j = 0; j < cornerCount; j++)
                    {
                        // renderer-local -> world -> reference-local
                        var worldCorner = localToWorld.MultiplyPoint3x4(localCorners[j]);
                        var refLocalCorner = worldToLocal.MultiplyPoint3x4(worldCorner);

                        if (initialized == false)
                        {
                            combined = new(refLocalCorner, Vector3.zero);
                            initialized = true;
                        }
                        else
                        {
                            combined.Encapsulate(refLocalCorner);
                        }
                    }
                }
            }

            return combined;
        }
    }
}
