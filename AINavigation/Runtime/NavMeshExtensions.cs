
using HisaCat.HUE.UnityExtensions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace HisaCat.HUE.AI.Navigation.Extensions
{
    public static class NavMeshExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            navMeshPathCache = new();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetPath([NotNull] NavMeshPath path, Vector3 from, Vector3 to, int passableMask)
        {
            // path.ClearCorners(); // Already cleared by NavMesh.CalculatePath()
            if (NavMesh.CalculatePath(from, to, passableMask, path) == false)
                return false;

            return true;
        }

        private static NavMeshPath navMeshPathCache = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFindClosestObjectIndex<T>(Vector3 from, IList<T> objects, System.Func<T, Vector3> getPosition, out int index, int passibleMask = NavMesh.AllAreas)
        {
            index = -1;

            float minDistance = float.MaxValue;
            var count = objects.SafeLength();
            for (int i = 0; i < count; i++)
            {
                var path = navMeshPathCache;
                if (TryGetPath(path, from, getPosition(objects[i]), passibleMask))
                {
                    if (path.TryGetPathLength(out var length) && length < minDistance)
                    {
                        minDistance = length;
                        index = i;
                    }
                }
            }

            return index != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFindClosestPointIndex(Vector3 from, IList<Vector3> points, out int index, int passibleMask = NavMesh.AllAreas)
            => TryFindClosestObjectIndex(from, points, static (point) => point, out index, passibleMask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TryFindClosestObject<T>(Vector3 from, IList<T> objects, System.Func<T, Vector3> getPosition, int passibleMask = NavMesh.AllAreas)
        {
            if (TryFindClosestObjectIndex(from, objects, getPosition, out var index, passibleMask))
                return objects[index];
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFindSafePosition(Vector3 position, out Vector3 safePosition, int passibleMask = NavMesh.AllAreas)
        {
            if (NavMesh.SamplePosition(position, out var hit, 1f, passibleMask))
            {
                safePosition = hit.position;
                return true;
            }

            safePosition = position;
            return false;
        }
    }

    public static class NavMeshPathExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetPathLength(this NavMeshPath path, out float length)
        {
            length = 0f;

            var cornerCount = path.corners.SafeLength();
            if (path.status == NavMeshPathStatus.PathInvalid || cornerCount <= 1)
                return false;

            for (int i = 0; i < cornerCount - 1; i++)
                length += Vector3.Distance(path.corners[i], path.corners[i + 1]);

            return true;
        }
    }

    public static class NavMeshAgentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFindClosestPointIndex(this NavMeshAgent agent, IList<Vector3> points, out int index, int passibleMask = NavMesh.AllAreas)
            => NavMeshExtensions.TryFindClosestPointIndex(agent.transform.position, points, out index, passibleMask);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TryFindClosestObject<T>(this NavMeshAgent agent, IList<T> objects, System.Func<T, Vector3> getPosition, int passibleMask = NavMesh.AllAreas)
            => NavMeshExtensions.TryFindClosestObject(agent.transform.position, objects, getPosition, passibleMask);
    }
}
