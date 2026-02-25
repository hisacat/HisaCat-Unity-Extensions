using HisaCat.HUE.Collections;
using HisaCat.HUE.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static partial class RTOcclusionManager
    {
        // TODO: 던전/룸 단위로 컬링 우선.. Occluder Room Group..
        // Initiliaze Culling.
        // Shadow까지 Culling되는 문제 또한 존재. 현재로써는 known issue로 둬도 될 것 같지만.

        public const bool IsActivated = false;

#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            OnEnterPlaymodeInEditor_Core(options);
            OnEnterPlaymodeInEditor_Culling(options);
            OnEnterPlaymodeInEditor_Preview(options);
        }
#pragma warning restore IDE0051
#endif

#if UNITY_EDITOR
#pragma warning disable IDE0051
        private static void OnEnterPlaymodeInEditor_Core(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                OcclusionCameraAdded = null;
                OcclusionCameraRemoved = null;

                OccluderAdded = null;
                OccluderRemoved = null;

                OccludeeAdded = null;
                OccludeeRemoved = null;

                occlusionCameras.Clear();
                occluders.Clear();
                occludees.Clear();

                CellsBuffer.Initialize();

                IsPreviewMode = false;
            }
        }
#pragma warning restore IDE0051
#endif

        #region Events
        public delegate void OcclusionCameraAddedCallback(RTOcclusionCamera occlusionCamera);
        public delegate void OcclusionCameraRemovedCallback(RTOcclusionCamera occlusionCamera);
        public static event OcclusionCameraAddedCallback OcclusionCameraAdded = null;
        public static event OcclusionCameraRemovedCallback OcclusionCameraRemoved = null;

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
        private static Dictionary<Vector3Int, HashSet<RTOccluder>> occluderOverlappedCells = new();
        public static IReadOnlyDictionary<Vector3Int, HashSet<RTOccluder>> OccluderOverlappedCells { get; private set; } = occluderOverlappedCells.AsReadOnly();
        private static Dictionary<Vector3Int, HashSet<RTOccludee>> occludeeOverlappedCells = new();
        public static IReadOnlyDictionary<Vector3Int, HashSet<RTOccludee>> OccludeeOverlappedCells { get; private set; } = occludeeOverlappedCells.AsReadOnly();
        #endregion Grids

        #region Buffers
        private readonly static StaticBuffer<Vector3Int> CellsBuffer = new(_ => _ = new Vector3Int[512]);
        #endregion Buffers

        private static readonly HashSet<RTOcclusionCamera> occlusionCameras = new();
        public static IReadOnlyHashSet<RTOcclusionCamera> OcclusionCameras { get; private set; } = occlusionCameras.AsReadOnly();
        private static readonly HashSet<RTOccluder> occluders = new();
        public static IReadOnlyHashSet<RTOccluder> Occluders { get; private set; } = occluders.AsReadOnly();
        private static readonly HashSet<RTOccludee> occludees = new();
        public static IReadOnlyHashSet<RTOccludee> Occludees { get; private set; } = occludees.AsReadOnly();

        // [NOTE] We using HashSet so duplicate checks are not required.
        // If we use another Collection, we need to check for duplicates.
        public static void RegisterOcclusionCamera(RTOcclusionCamera occlusionCamera)
        {
            occlusionCameras.Add(occlusionCamera);
            OcclusionCameraAdded?.Invoke(occlusionCamera);

            MarkAsUpdateCulling();
        }
        public static void UnregisterOcclusionCamera(RTOcclusionCamera occlusionCamera)
        {
            occlusionCameras.Remove(occlusionCamera);
            OcclusionCameraRemoved?.Invoke(occlusionCamera);

            MarkAsUpdateCulling();
        }

        public static void RegisterOccluder(RTOccluder occluder)
        {
            // // Set culling state to true when registering.
            if (Application.isPlaying)
            {
                // if (occluder.EnableCullingSelf)
                // {
                //     occluder.SetCulling(true);
                //     culledOccluders.Add(occluder);
                // }
            }

            occluders.Add(occluder);
            OccluderAdded?.Invoke(occluder);
            UpdateOccluderOverlappedCells(occluder); // Update overlapped cells.

            MarkAsUpdateCulling(); // Mark as update culling state.
        }
        public static void UnregisterOccluder(RTOccluder occluder)
        {
            // Restore culling state when unregistering.
            // if (occluder.EnableCullingSelf)
            // {
            //     occluder.SetCulling(false);
            //     culledOccluders.Remove(occluder);
            // }

            occluders.Remove(occluder);
            OccluderRemoved?.Invoke(occluder);

            MarkAsUpdateCulling(); // Mark as update culling state.
        }
        public static void RegisterOccludee(RTOccludee occludee)
        {
            #pragma warning disable CS0162 // Unreachable code detected
            if (RTOcclusionManager.IsActivated)
            {
                // Set culling state to true when registering.
                if (Application.isPlaying)
                {
                    occludee.SetCulling(true);
                    culledOccludees.Add(occludee);
                }
            }
            #pragma warning restore CS0162 // Unreachable code detected

            occludees.Add(occludee);
            OccludeeAdded?.Invoke(occludee);
            UpdateOccludeeOverlappedCells(occludee); // Update overlapped cells.

            MarkAsUpdateCulling(); // Mark as update culling state.
        }
        public static void UnregisterOccludee(RTOccludee occludee)
        {
            // Restore culling state when unregistering.
            occludee.SetCulling(false);
            culledOccludees.Remove(occludee);

            occludees.Remove(occludee);
            OccludeeRemoved?.Invoke(occludee);

            MarkAsUpdateCulling(); // Mark as update culling state.
        }

        public static void OnOcclusionCameraTransformChanged(RTOcclusionCamera occlusionCamera)
        {
            MarkAsUpdateCulling(); // Mark as update culling state.
        }
        public static void OnOccluderTransformChanged(RTOccluder occluder)
        {
            UpdateOccluderOverlappedCells(occluder); // Update overlapped cells.

            MarkAsUpdateCulling(); // Mark as update culling state.
        }
        public static void OnOccludeeTransformChanged(RTOccludee occludee)
        {
            UpdateOccludeeOverlappedCells(occludee); // Update overlapped cells.

            MarkAsUpdateCulling(); // Mark as update culling state.
        }

        public static void UpdateOccluderOverlappedCells(RTOccluder occluder) => UpdateOverlappedCells(occluder, occluderOverlappedCells);
        public static void UpdateOccludeeOverlappedCells(RTOccludee occludee) => UpdateOverlappedCells(occludee, occludeeOverlappedCells);
        private static void UpdateOverlappedCells<TTarget>(TTarget target, Dictionary<Vector3Int, HashSet<TTarget>> grid) where TTarget : RTOcclusionBase
        {
            // Remove origin cells
            {
                var cells = target.OverlappedCells;
                using (var enumerator = cells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var cell = enumerator.Current;
                        if (grid.ContainsKey(cell) == false) continue;

                        grid[cell].Remove(target);
                    }
                }
            }

            var count = GetOverlappedCellsNonAlloc(
                target.Bounds,
                target.CachedLocalToWorldMatrix, CellSize,
                CellsBuffer);
            target.UpdateOverlappedCells(CellsBuffer, count);

            // Add new cells
            {
                var cells = target.OverlappedCells;
                using (var enumerator = cells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var cell = enumerator.Current;
                        if (grid.ContainsKey(cell) == false) grid.Add(cell, new HashSet<TTarget>());

                        grid[cell].Add(target);
                    }
                }
            }
        }

        // [NOTE] 현재는 XYZ 축 기준 AABB의 min/max 인덱스 범위로 셀을 산출하므로, 결과 영역은 항상 그리드 정렬 직육면체가 됩니다.
        // 실제로 교차하는 셀만 엄밀히 산출(예: OBB-셀 교차 테스트)할 수 있으나, 성능상 현재 방식을 기본으로 유지합니다.
        public static int GetOverlappedCellsNonAlloc(Bounds localBounds, Matrix4x4 localToWorld, float gridSize, StaticBuffer<Vector3Int> cellsBuffer)
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
                cellsBuffer.Buffer[0] = minIndex;
                return 1;
            }

            int count = 0;
            for (int x = minIndex.x; x <= maxIndex.x; x++)
            {
                for (int y = minIndex.y; y <= maxIndex.y; y++)
                {
                    for (int z = minIndex.z; z <= maxIndex.z; z++)
                    {
                        cellsBuffer.AddItemSafely(ref count, new Vector3Int(x, y, z));
                    }
                }
            }

            return count;
        }

    }
}
