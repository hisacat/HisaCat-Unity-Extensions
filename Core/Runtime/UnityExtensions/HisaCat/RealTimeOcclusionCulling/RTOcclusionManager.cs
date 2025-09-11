using HisaCat.UnityExtensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static class RTOcclusionManager
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                Occluders.Clear();
                Occludees.Clear();

                CellsBuffer.Initialize();
            }
        }
#pragma warning restore IDE0051
#endif

        #region Events
        public delegate void OccluderAddedCallback(RTOccluder occluder);
        public delegate void OccluderRemovedCallback(RTOccluder occluder);
        public static event OccluderAddedCallback OccluderAdded = null;
        public static event OccluderRemovedCallback OccluderRemoved = null;

        public delegate void OccludeeAddedCallback(RTOccludee occludee);
        public delegate void OccludeeRemovedCallback(RTOccludee occludee);
        public static event OccludeeAddedCallback OccludeeAdded = null;
        public static event OccludeeRemovedCallback OccludeeRemoved = null;
        #endregion Events

        #region Grids
        public const float CellSize = 5f;
        private static Dictionary<Vector3Int, HashSet<RTOccluder>> OccluderOverlappedCells = new();
        private static Dictionary<Vector3Int, HashSet<RTOccludee>> OccludeeOverlappedCells = new();
        #endregion Grids

        #region Buffers
        private readonly static StaticBuffer<Vector3Int> CellsBuffer = new(_ => _ = new Vector3Int[512]);
        #endregion Buffers

        private static readonly HashSet<RTOccluder> Occluders = new();
        private static readonly HashSet<RTOccludee> Occludees = new();

        // [NOTE] We using HashSet so duplicate checks are not required.
        // If we use another Collection, we need to check for duplicates.
        public static void RegisterOccluder(RTOccluder occluder)
        {
            Occluders.Add(occluder);
            OccluderAdded?.Invoke(occluder);
            UpdateOccluderOverlappedCells(occluder);
        }
        public static void UnregisterOccluder(RTOccluder occluder)
        {
            Occluders.Remove(occluder);
            OccluderRemoved?.Invoke(occluder);
            // ...
        }
        public static void RegisterOccludee(RTOccludee occludee)
        {
            Occludees.Add(occludee);
            OccludeeAdded?.Invoke(occludee);
            UpdateOccludeeOverlappedCells(occludee);
        }
        public static void UnregisterOccludee(RTOccludee occludee)
        {
            Occludees.Remove(occludee);
            OccludeeRemoved?.Invoke(occludee);
            // ...
        }

        public static void OnOccluderTransformChanged(RTOccluder occluder)
        {
            UpdateOccluderOverlappedCells(occluder);
        }
        public static void OnOccludeeTransformChanged(RTOccludee occludee)
        {
            UpdateOccludeeOverlappedCells(occludee);
        }

        public static void UpdateOccluderOverlappedCells(RTOccluder occluder)
        {
            UpdateOverlappedCells(occluder, OccluderOverlappedCells);
        }
        public static void UpdateOccludeeOverlappedCells(RTOccludee occludee)
        {
            UpdateOverlappedCells(occludee, OccludeeOverlappedCells);
        }
        private static void UpdateOverlappedCells<TTarget>(TTarget target, Dictionary<Vector3Int, HashSet<TTarget>> grid) where TTarget : RTOcclusionBase
        {
            // Remove origin cells
            {
                var cells = target.OverlappedCells;
                using (var enumerator = cells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        grid[enumerator.Current].Remove(target);
                }
            }

            var count = GetOverlappedCellsNonAlloc(
                target.Bounds,
                target.transform.localToWorldMatrix, CellSize,
                CellsBuffer.Buffer);
            target.UpdateOverlappedCells(CellsBuffer.Buffer, count);

            // Add new cells
            {
                var cells = target.OverlappedCells;
                using (var enumerator = cells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        grid[enumerator.Current].Add(target);
                }
            }
        }

        // [NOTE] 현재는 XYZ 축 기준 AABB의 min/max 인덱스 범위로 셀을 산출하므로, 결과 영역은 항상 그리드 정렬 직육면체가 됩니다.
        // 실제로 교차하는 셀만 엄밀히 산출(예: OBB-셀 교차 테스트)할 수 있으나, 성능상 현재 방식을 기본으로 유지합니다.
        public static int GetOverlappedCellsNonAlloc(Bounds localBounds, Matrix4x4 localToWorld, float gridSize, Vector3Int[] cells)
        {
            // Build 8 local-space corners of the AABB
            var c = localBounds.center;
            var e = localBounds.extents;

            var localCorners = new Vector3[8]
            {
                new (c.x - e.x, c.y - e.y, c.z - e.z),
                new (c.x + e.x, c.y - e.y, c.z - e.z),
                new (c.x - e.x, c.y + e.y, c.z - e.z),
                new (c.x + e.x, c.y + e.y, c.z - e.z),
                new (c.x - e.x, c.y - e.y, c.z + e.z),
                new (c.x + e.x, c.y - e.y, c.z + e.z),
                new (c.x - e.x, c.y + e.y, c.z + e.z),
                new (c.x + e.x, c.y + e.y, c.z + e.z)
            };

            // Transform corners to world space and compute world AABB
            Vector3 minW = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 maxW = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < 8; i++)
            {
                var w = localToWorld.MultiplyPoint3x4(localCorners[i]);
                minW = Vector3.Min(minW, w);
                maxW = Vector3.Max(maxW, w);
            }

            // Convert world AABB to grid index range
            const float eps = 1e-5f; // avoid including an extra cell when exactly on boundary
            var minIndex = new Vector3Int(
                Mathf.FloorToInt(minW.x / gridSize),
                Mathf.FloorToInt(minW.y / gridSize),
                Mathf.FloorToInt(minW.z / gridSize)
            );
            var maxIndex = new Vector3Int(
                Mathf.FloorToInt((maxW.x - eps) / gridSize),
                Mathf.FloorToInt((maxW.y - eps) / gridSize),
                Mathf.FloorToInt((maxW.z - eps) / gridSize)
            );

            if (maxIndex.x < minIndex.x || maxIndex.y < minIndex.y || maxIndex.z < minIndex.z)
            {
                // Degenerate bounds: treat as a single cell at minIndex
                cells[0] = minIndex;
                return 1;
            }

            int count = 0;
            for (int x = minIndex.x; x <= maxIndex.x; x++)
            {
                for (int y = minIndex.y; y <= maxIndex.y; y++)
                {
                    for (int z = minIndex.z; z <= maxIndex.z; z++)
                    {
                        if (count >= cells.Length)
                        {
                            Debug.LogWarning($"Buffer reached max size. Stopping calculation.");
                            break;
                        }
                        cells[count++] = new Vector3Int(x, y, z);
                    }
                }
            }

            return count;
        }
    }
}