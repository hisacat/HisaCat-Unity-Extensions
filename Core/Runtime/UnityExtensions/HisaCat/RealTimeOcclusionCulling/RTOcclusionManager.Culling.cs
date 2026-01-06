using HisaCat.HUE.Collections;
using HisaCat.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Profiling;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static partial class RTOcclusionManager
    {
        private static void OnEnterPlaymodeInEditor_Culling(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                culledOccludees.Clear();

                WillUpdateCulling = false;

                // 버퍼 초기화
                sortedCellsBuffer.Clear();
                sortedOccludersBuffer.Clear();
            }
        }

        #region Buffers
        // 거리 정렬용 재사용 가능 버퍼
        private readonly static List<(Vector3Int cell, float distanceSqr)> sortedCellsBuffer = new();
        private readonly static List<(RTOccluder occluder, float distanceSqr)> sortedOccludersBuffer = new();
        #endregion Buffers

        private readonly static HashSet<RTOccludee> culledOccludees = new();
        public static IReadOnlyHashSet<RTOccludee> CulledOccludees { get; private set; } = culledOccludees.AsReadOnly();
        private readonly static HashSet<RTOccluder> culledOccluders = new();
        public static IReadOnlyHashSet<RTOccluder> CulledOccluders { get; private set; } = culledOccluders.AsReadOnly();

        public static bool WillUpdateCulling { get; private set; } = false;
        public static void MarkAsUpdateCulling() => WillUpdateCulling = true;

        #region Performance Stats
        public struct CullingStats
        {
            public int TotalOccludees;
            public int CulledOccludees;
            public int TotalOccluders;
            public int CulledOccluders;
            public int VisibleCells;
            public float LastUpdateTimeMs;

            public override readonly string ToString()
            {
                return $"RTOcclusion Stats:\n" +
                       $"  Occludees: {culledOccludees}/{TotalOccludees} culled\n" +
                       $"  Occluders: {culledOccluders}/{TotalOccluders} culled\n" +
                       $"  Visible Cells: {VisibleCells}\n" +
                       $"  Update Time: {LastUpdateTimeMs:F2}ms";
            }
        }

        private static CullingStats currentStats;
        public static CullingStats CurrentStats => currentStats;
        #endregion Performance Stats

        public static void UpdateCullingIfRequired()
        {
            if (WillUpdateCulling)
            {
                WillUpdateCulling = false;
                ForceUpdateCulling();
            }
        }

        public static void ForceUpdateCulling()
        {
            Profiler.BeginSample("RTOcclusion.ForceUpdateCulling");
            var startTime = Time.realtimeSinceStartup;

            // Stats 초기화
            currentStats = new CullingStats
            {
                TotalOccludees = Occludees.Count,
                TotalOccluders = Occluders.Count,
                CulledOccludees = culledOccludees.Count,
                CulledOccluders = culledOccluders.Count,
                VisibleCells = 0
            };

            // ============================================================
            // [초기화] 이전에 Cull된 Occludee들을 복원
            // ============================================================
            Profiler.BeginSample("1. Reset Occludees");
            using (var enumerator = culledOccludees.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var occludee = enumerator.Current;
                    occludee.SetCulling(false); // Culling 해제
                }
            }
            culledOccludees.Clear();
            culledOccludees.AddRange(Occludees); // 모든 Occludee를 후보로 추가
            Profiler.EndSample();

            // ============================================================
            // [메인 루프] 카메라 시점에서 보이지 않는 Occludee 제거
            // ============================================================
            using (var cameraEnumerator = OcclusionCameras.GetEnumerator())
            {
                while (cameraEnumerator.MoveNext())
                {
                    var occlusionCamera = cameraEnumerator.Current;
                    var camera = occlusionCamera.Camera;
                    if (camera == null) continue;

                    // ============================================================
                    // [PHASE 1] 카메라 정보 수집
                    // ============================================================
                    Profiler.BeginSample("2. Camera Setup");
                    var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
                    var cameraPos = occlusionCamera.CameraTransformCache.position;
                    var cameraForward = occlusionCamera.CameraTransformCache.forward;
                    var nearClipPlane = camera.nearClipPlane;
                    var farClipPlane = camera.farClipPlane;
                    Profiler.EndSample();

                    // ============================================================
                    // [PHASE 2] Cell을 거리순으로 정렬하여 가까운 것부터 처리
                    // ============================================================
                    Profiler.BeginSample("3. Sort Cells by Distance");
                    sortedCellsBuffer.Clear();

                    foreach (var cellEntry in OccluderOverlappedCells)
                    {
                        var cell = cellEntry.Key;
                        var cellBounds = RTOcclusionUtility.GetCellWorldBounds(cell, CellSize);

                        // 프러스텀 체크
                        if (GeometryUtility.TestPlanesAABB(frustumPlanes, cellBounds) == false)
                            continue;

                        // 카메라로부터 Cell 중심까지의 거리 제곱 계산
                        var distanceSqr = (cellBounds.center - cameraPos).sqrMagnitude;
                        sortedCellsBuffer.Add((cell, distanceSqr));
                    }

                    // 거리순 정렬 (가까운 것부터)
                    sortedCellsBuffer.Sort((a, b) => a.distanceSqr.CompareTo(b.distanceSqr));
                    currentStats.VisibleCells = sortedCellsBuffer.Count;
                    Profiler.EndSample();

                    // ============================================================
                    // [PHASE 3] Occludee 가시성 판정 (최적화된 버전)
                    // ============================================================
                    Profiler.BeginSample("4. Check Occludee Visibility");
                    const float epsilon = 0.001f;
                    var occludeesToCheck = new List<RTOccludee>(culledOccludees);

                    foreach (var occludee in occludeesToCheck)
                    {
                        if (occludee == null) continue;

                        // [Step A] 프러스텀 및 거리 체크 (캐싱된 Bounds 사용)
                        var occludeeWorldBounds = occludee.CachedWorldBounds;
                        var occludeeWorldCorners = occludee.CachedWorldCorners;

                        if (GeometryUtility.TestPlanesAABB(frustumPlanes, occludeeWorldBounds) == false)
                            continue;

                        var occludeeCenter = occludeeWorldBounds.center;
                        var depth = Vector3.Dot(occludeeCenter - cameraPos, cameraForward);
                        if (depth < nearClipPlane || depth > farClipPlane)
                            continue;

                        // [Step B] 8-코너 가시성 체크 (거리 기반 최적화)
                        bool isVisible = false;

                        for (int cornerIdx = 0; cornerIdx < 8; cornerIdx++)
                        {
                            var cornerPos = occludeeWorldCorners[cornerIdx];
                            var cornerDirection = (cornerPos - cameraPos);
                            var cornerDistanceSqr = cornerDirection.sqrMagnitude;
                            var cornerDistance = Mathf.Sqrt(cornerDistanceSqr);
                            var ray = new Ray(cameraPos, cornerDirection / cornerDistance);

                            bool cornerBlocked = false;

                            // Cell을 가까운 순서대로 체크
                            for (int cellIdx = 0; cellIdx < sortedCellsBuffer.Count; cellIdx++)
                            {
                                var (cell, cellDistSqr) = sortedCellsBuffer[cellIdx];

                                // Early Exit: Cell이 코너보다 멀리 있으면 더 이상 체크 불필요
                                if (cellDistSqr > cornerDistanceSqr)
                                    break;

                                if (OccluderOverlappedCells.ContainsKey(cell) == false)
                                    continue;

                                // Occluder를 거리순으로 정렬
                                sortedOccludersBuffer.Clear();
                                foreach (var occluder in OccluderOverlappedCells[cell])
                                {
                                    if (occluder == null) continue;

                                    var occluderPos = occluder.TransformCache.position;
                                    var occluderDistSqr = (occluderPos - cameraPos).sqrMagnitude;

                                    // Occluder가 코너보다 가까운 경우만 체크
                                    if (occluderDistSqr < cornerDistanceSqr)
                                    {
                                        sortedOccludersBuffer.Add((occluder, occluderDistSqr));
                                    }
                                }

                                // 거리순 정렬
                                sortedOccludersBuffer.Sort((a, b) => a.distanceSqr.CompareTo(b.distanceSqr));

                                // 가까운 Occluder부터 체크
                                for (int occluderIdx = 0; occluderIdx < sortedOccludersBuffer.Count; occluderIdx++)
                                {
                                    var occluder = sortedOccludersBuffer[occluderIdx].occluder;

                                    // 캐싱된 Bounds 사용 (성능 최적화)
                                    var occluderWorldBounds = occluder.CachedWorldBounds;

                                    if (occluderWorldBounds.IntersectRay(ray, out float hitDistance))
                                    {
                                        if (hitDistance < cornerDistance - epsilon)
                                        {
                                            bool passedThroughPortal = RTOcclusionUtility.CheckRayPassesThroughPortal(
                                                ray, occluder, useDetailedPortalCheck: true);

                                            if (passedThroughPortal == false)
                                            {
                                                cornerBlocked = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (cornerBlocked) break;
                            }

                            if (cornerBlocked == false)
                            {
                                isVisible = true;
                                break; // 하나라도 보이면 즉시 종료
                            }
                        }

                        if (isVisible)
                        {
                            culledOccludees.Remove(occludee);
                        }
                    }
                    Profiler.EndSample(); // 4. Check Occludee Visibility
                }
            }

            // ============================================================
            // [마무리] Cull된 Occludee들을 실제로 숨김 처리
            // ============================================================
            Profiler.BeginSample("5. Apply Culling");
            using (var enumerator = culledOccludees.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var occludee = enumerator.Current;
                    occludee.SetCulling(true);
                }
            }
            Profiler.EndSample();

            // Stats 마무리
            currentStats.CulledOccludees = culledOccludees.Count;
            currentStats.CulledOccluders = culledOccluders.Count;
            currentStats.LastUpdateTimeMs = (Time.realtimeSinceStartup - startTime) * 1000f;

            Profiler.EndSample(); // RTOcclusion.ForceUpdateCulling
        }
    }
}
