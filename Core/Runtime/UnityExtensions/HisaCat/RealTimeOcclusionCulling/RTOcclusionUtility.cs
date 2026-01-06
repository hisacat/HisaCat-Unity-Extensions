using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static class RTOcclusionUtility
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                WorldRectCornersBuffer.Initialize();
            }
        }
#pragma warning restore IDE0051
#endif

        #region Buffers
        private readonly static StaticBuffer<Vector3> WorldRectCornersBuffer = new(_ => new Vector3[4]);
        #endregion Buffers

        /// <summary>
        /// 주어진 렌더러들의 결합된 축 정렬 경계 상자(AABB)를 계산합니다.
        /// 결과는 <paramref name="referenceTransform"/>의 로컬 좌표계로 표현됩니다.
        /// 각 렌더러의 로컬 경계를 월드 공간으로 변환한 후 참조 Transform의 로컬 공간으로 변환하여
        /// (8개 코너 포인트를 통해) 회전과 비균등 스케일을 올바르게 고려합니다.
        /// </summary>
        /// <param name="referenceTransform">결과가 표현될 로컬 공간을 정의하는 Transform</param>
        /// <param name="renderers">결합 경계를 계산할 때 포함할 렌더러 목록</param>
        /// <returns>
        /// <paramref name="referenceTransform"/> 로컬 공간의 결합된 AABB, 유효한 렌더러가 없으면 기본값 반환
        /// </returns>
        public static Bounds CalculateRenderersCombinedLocalBounds(Transform referenceTransform, List<Renderer> renderers)
        {
            if (renderers == null) return default;
            if (renderers.Count == 0) return default;

            // 활성화된 렌더러만 필터링 (비활성/숨겨진 오브젝트 제외)
            var activeRenderers = new List<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.gameObject.activeInHierarchy)
                    activeRenderers.Add(renderer);
            }

            if (activeRenderers.Count == 0) return default;

            // 참조 Transform의 로컬 공간에서 AABB 계산
            // 각 렌더러마다:
            //   렌더러 로컬 좌표(경계 코너) -> 월드 좌표 -> 참조 로컬 좌표로 변환 후 병합
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
                    // AABB의 8개 코너 계산 (중심점 + 각 축의 범위로부터)
                    var localCorners = new Vector3[cornerCount]
                    {
                            new (c.x - e.x, c.y - e.y, c.z - e.z), // 0: 왼쪽 아래 뒤 (-X, -Y, -Z)
                            new (c.x + e.x, c.y - e.y, c.z - e.z), // 1: 오른쪽 아래 뒤 (+X, -Y, -Z)
                            new (c.x - e.x, c.y + e.y, c.z - e.z), // 2: 왼쪽 위 뒤 (-X, +Y, -Z)
                            new (c.x + e.x, c.y + e.y, c.z - e.z), // 3: 오른쪽 위 뒤 (+X, +Y, -Z)
                            new (c.x - e.x, c.y - e.y, c.z + e.z), // 4: 왼쪽 아래 앞 (-X, -Y, +Z)
                            new (c.x + e.x, c.y - e.y, c.z + e.z), // 5: 오른쪽 아래 앞 (+X, -Y, +Z)
                            new (c.x - e.x, c.y + e.y, c.z + e.z), // 6: 왼쪽 위 앞 (-X, +Y, +Z)
                            new (c.x + e.x, c.y + e.y, c.z + e.z)  // 7: 오른쪽 위 앞 (+X, +Y, +Z)
                    };

                    // 렌더러의 로컬 좌표를 월드 좌표로 변환하는 매트릭스
                    var localToWorld = renderer.localToWorldMatrix;
                    for (int j = 0; j < cornerCount; j++)
                    {
                        // 렌더러 로컬 -> 월드 -> 참조 로컬 좌표계로 변환
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

        /// <summary>
        /// 셀 좌표로부터 월드 공간의 AABB를 계산합니다.
        /// 셀 좌표와 셀 크기를 이용하여 해당 셀의 월드 경계 상자를 반환합니다.
        /// </summary>
        /// <param name="cell">셀의 3D 그리드 좌표</param>
        /// <param name="cellSize">셀 하나의 크기</param>
        /// <returns>월드 공간에서의 셀 경계 상자</returns>
        public static Bounds GetCellWorldBounds(Vector3Int cell, float cellSize)
        {
            Vector3 cellWorldPos = new Vector3(cell.x * cellSize, cell.y * cellSize, cell.z * cellSize);
            Vector3 center = cellWorldPos + Vector3.one * cellSize * 0.5f;
            Vector3 size = Vector3.one * cellSize;
            return new Bounds(center, size);
        }

        /// <summary>
        /// 월드 공간의 위치로부터 셀 좌표를 계산합니다.
        /// </summary>
        /// <param name="worldPosition">월드 공간의 위치</param>
        /// <param name="cellSize">셀 하나의 크기</param> 
        /// <returns>셀 좌표</returns>
        public static Vector3Int CalculateCellFromWorldPosition(Vector3 worldPosition, float cellSize)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / cellSize),
                Mathf.FloorToInt(worldPosition.y / cellSize),
                Mathf.FloorToInt(worldPosition.z / cellSize)
            );
        }

        /// <summary>
        /// 로컬 AABB의 8개 코너를 계산하고 월드 공간으로 변환합니다.
        /// 메모리 할당 없이 주어진 버퍼에 결과를 저장합니다 (성능 최적화).
        /// </summary>
        /// <param name="localBounds">로컬 공간의 경계 상자</param>
        /// <param name="localToWorld">로컬 좌표를 월드 좌표로 변환하는 매트릭스</param>
        /// <param name="cornersBuffer">결과를 저장할 Vector3 배열 (최소 8개 크기)</param>
        public static void CalculateWorldCornersNonAlloc(Bounds localBounds, Matrix4x4 localToWorld, Vector3[] cornersBuffer)
        {
            var c = localBounds.center;
            var e = localBounds.extents;

            // AABB의 8개 코너를 로컬 공간에서 월드 공간으로 변환
            cornersBuffer[0] = localToWorld.MultiplyPoint3x4(new Vector3(c.x - e.x, c.y - e.y, c.z - e.z)); // 0: 왼쪽 아래 뒤 (-X, -Y, -Z)
            cornersBuffer[1] = localToWorld.MultiplyPoint3x4(new Vector3(c.x + e.x, c.y - e.y, c.z - e.z)); // 1: 오른쪽 아래 뒤 (+X, -Y, -Z)
            cornersBuffer[2] = localToWorld.MultiplyPoint3x4(new Vector3(c.x - e.x, c.y + e.y, c.z - e.z)); // 2: 왼쪽 위 뒤 (-X, +Y, -Z)
            cornersBuffer[3] = localToWorld.MultiplyPoint3x4(new Vector3(c.x + e.x, c.y + e.y, c.z - e.z)); // 3: 오른쪽 위 뒤 (+X, +Y, -Z)
            cornersBuffer[4] = localToWorld.MultiplyPoint3x4(new Vector3(c.x - e.x, c.y - e.y, c.z + e.z)); // 4: 왼쪽 아래 앞 (-X, -Y, +Z)
            cornersBuffer[5] = localToWorld.MultiplyPoint3x4(new Vector3(c.x + e.x, c.y - e.y, c.z + e.z)); // 5: 오른쪽 아래 앞 (+X, -Y, +Z)
            cornersBuffer[6] = localToWorld.MultiplyPoint3x4(new Vector3(c.x - e.x, c.y + e.y, c.z + e.z)); // 6: 왼쪽 위 앞 (-X, +Y, +Z)
            cornersBuffer[7] = localToWorld.MultiplyPoint3x4(new Vector3(c.x + e.x, c.y + e.y, c.z + e.z)); // 7: 오른쪽 위 앞 (+X, +Y, +Z)
        }

        /// <summary>
        /// 월드 공간의 코너 포인트들로부터 축 정렬 경계 상자(AABB)를 계산합니다.
        /// 모든 코너를 포함하는 최소 크기의 AABB를 반환합니다.
        /// 성능 최적화: 수동 Min/Max 계산으로 Vector3.Min/Max 호출 제거
        /// </summary>
        /// <param name="worldCorners">월드 공간의 코너 포인트 배열 (8개)</param>
        /// <returns>모든 코너를 포함하는 월드 공간의 AABB</returns>
        public static Bounds CalculateWorldAABBFromCorners(Vector3[] worldCorners)
        {
            // 예전 로직
            {
                // Vector3 min = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                // Vector3 max = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

                // for (int i = 0; i < 8; i++)
                // {
                //     min = Vector3.Min(min, worldCorners[i]);
                //     max = Vector3.Max(max, worldCorners[i]);
                // }

                // Vector3 center = (min + max) * 0.5f;
                // Vector3 size = max - min;
                // return new Bounds(center, size);
            }

            // 최적화된 로직
            {
                // 첫 번째 코너로 초기화
                float minX = worldCorners[0].x, minY = worldCorners[0].y, minZ = worldCorners[0].z;
                float maxX = worldCorners[0].x, maxY = worldCorners[0].y, maxZ = worldCorners[0].z;

                // 나머지 코너들로 min/max 갱신 (Vector3.Min/Max 대신 수동 계산)
                for (int i = 1; i < 8; i++)
                {
                    var corner = worldCorners[i];

                    if (corner.x < minX) minX = corner.x;
                    else if (corner.x > maxX) maxX = corner.x;

                    if (corner.y < minY) minY = corner.y;
                    else if (corner.y > maxY) maxY = corner.y;

                    if (corner.z < minZ) minZ = corner.z;
                    else if (corner.z > maxZ) maxZ = corner.z;
                }

                // 중심과 크기 계산
                float centerX = (minX + maxX) * 0.5f;
                float centerY = (minY + maxY) * 0.5f;
                float centerZ = (minZ + maxZ) * 0.5f;

                float sizeX = maxX - minX;
                float sizeY = maxY - minY;
                float sizeZ = maxZ - minZ;

                return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
            }
        }


        /// <summary>
        /// Ray가 주어진 Occluder의 활성화된 Portal을 통과하는지 확인합니다.
        /// Portal을 통과하면 true, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <param name="ray">검사할 Ray (시작점과 방향)</param>
        /// <param name="occluder">Portal을 포함하는 Occluder</param>
        /// <param name="useDetailedPortalCheck">
        /// true일 경우: 정확한 2D 평면 교차 검사 수행 (느리지만 정확)<br/>
        /// false일 경우: 빠른 3D AABB 검사 수행 (덜 정확하지만 성능 좋음)
        /// </param>
        /// <returns>Ray가 Portal을 통과하면 true, 아니면 false</returns>
        public static bool CheckRayPassesThroughPortal(Ray ray, RTOccluder occluder, bool useDetailedPortalCheck)
        {
            var portals = occluder.FacePortals;
            if (portals == null || portals.Count == 0) return false;

            // 모든 Portal을 순회하며 Ray와의 교차 검사
            for (int i = 0; i < portals.Count; i++)
            {
                var portal = portals[i];
                if (portal == null || portal.IsEnabled == false) continue; // 비활성화된 Portal은 무시

                if (useDetailedPortalCheck)
                {
                    // // [방법1] 정확한 Rect 교차 검사
                    // // Portal을 앞면과 뒷면의 사각형으로 나누어 각각 모두 통과하는지 검사
                    var portalWorldCorners = occluder.GetFacePortalWorldCornersCache(portal);

                    var frontRectCorners = WorldRectCornersBuffer.Buffer;
                    GetPortalWorldRectCornersNonAlloc(portalWorldCorners, portal.Face, isFront: true, frontRectCorners);
                    if (CheckRayIntersectsWorldRect(ray, frontRectCorners[0], frontRectCorners[1], frontRectCorners[2], frontRectCorners[3]) == false)
                        return false;

                    var backRectCorners = WorldRectCornersBuffer.Buffer; GetPortalWorldRectCornersNonAlloc(portalWorldCorners, portal.Face, isFront: false, backRectCorners);
                    GetPortalWorldRectCornersNonAlloc(portalWorldCorners, portal.Face, isFront: false, backRectCorners);
                    if (CheckRayIntersectsWorldRect(ray, backRectCorners[0], backRectCorners[1], backRectCorners[2], backRectCorners[3]) == false)
                        return false;

                    return true;
                }
                else
                {
                    // [방법 2] 빠른 3D AABB 체크 (덜 정확)
                    // Portal을 3D 박스로 근사하여 검사 (Portal 두께가 고려됨)
                    // 단점: Portal 영역보다 큰 박스로 검사하여 부정확
                    var portalWorldBounds = occluder.GetFacePortalWorldBoundsCache(portal);
                    if (portalWorldBounds.IntersectRay(ray, out _))
                        return true; // 하나라도 통과하면 성공
                }
            }

            return false; // 모든 Portal을 통과하지 못함
        }

        /// <summary>
        /// Portal의 월드 공간 사각형 코너를 계산합니다.
        /// </summary>
        /// <param name="portal">계산할 Portal</param>
        /// <param name="occluder">Portal이 속한 Occluder</param>
        /// <param name="isFront">true면 앞면(바깥쪽), false면 뒷면(안쪽)</param>
        /// <returns>Portal의 월드 공간 사각형 코너 (CCW: 좌하, 우하, 우상, 좌상 순서)</returns>
        private static void GetPortalWorldRectCornersNonAlloc(Vector3[] worldCorners, RTBoundsFace face, bool isFront, Vector3[] worldRectCornersBuffer)
        {
            void Insert(int lbIdx, int rbIdx, int rtIdx, int ltIdx)
            {
                worldRectCornersBuffer[0] = worldCorners[lbIdx];
                worldRectCornersBuffer[1] = worldCorners[rbIdx];
                worldRectCornersBuffer[2] = worldCorners[rtIdx];
                worldRectCornersBuffer[3] = worldCorners[ltIdx];
            }
            // 각 면에서 바라봤을 때 CCW(반시계방향) 순서로 반환
            switch (face)
            {
                case RTBoundsFace.X: // +X 면 (오른쪽 면) - YZ 평면
                    if (isFront) // 앞면 (바깥쪽, +X 방향)
                        Insert(1, 5, 7, 4); // 좌하(1), 우하(5), 우상(7), 좌상(3)
                    else // 뒷면 (안쪽, -X 방향)
                        Insert(0, 4, 6, 2); // 좌하(0), 우하(4), 우상(6), 좌상(2)
                    break;
                case RTBoundsFace.Y: // +Y 면 (위쪽 면) - XZ 평면
                    if (isFront) // 앞면 (바깥쪽, +Y 방향)
                        Insert(2, 3, 7, 6); // 좌하(2), 우하(3), 우상(7), 좌상(6)
                    else // 뒷면 (안쪽, -Y 방향)
                        Insert(0, 1, 5, 4); // 좌하(0), 우하(1), 우상(5), 좌상(4)
                    break;
                case RTBoundsFace.Z: // +Z 면 (앞쪽 면) - XY 평면
                default:
                    if (isFront) // 앞면 (바깥쪽, +Z 방향)
                        Insert(4, 5, 7, 6); // 좌하(4), 우하(5), 우상(7), 좌상(6)
                    else // 뒷면 (안쪽, -Z 방향)
                        Insert(0, 1, 3, 2); // 좌하(0), 우하(1), 우상(3), 좌상(2)
                    break;
            }
        }

        /// <summary>
        /// Ray가 월드 공간의 2D 사각형과 교차하는지 검사합니다.
        /// 4개의 코너로 정의된 사각형과 Ray의 교차를 판정합니다.
        /// </summary>
        /// <param name="ray">검사할 Ray</param>
        /// <param name="lb">좌하단 코너 (월드 공간)</param>
        /// <param name="rb">우하단 코너 (월드 공간)</param>
        /// <param name="rt">우상단 코너 (월드 공간)</param>
        /// <param name="lt">좌상단 코너 (월드 공간)</param>
        /// <returns>Ray가 사각형과 교차하면 true, 아니면 false</returns>
        private static bool CheckRayIntersectsWorldRect(Ray ray, Vector3 lb, Vector3 rb, Vector3 rt, Vector3 lt)
        {
            // 1. 평면의 노멀 벡터 계산 (두 변의 외적)
            Vector3 edge1 = rb - lb; // 하단 변 (좌 -> 우)
            Vector3 edge2 = lt - lb; // 좌측 변 (하 -> 상)
            Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

            // 2. Ray와 평면의 교차 계산
            float denom = Vector3.Dot(normal, ray.direction);
            if (Mathf.Abs(denom) < 1e-6f) return false; // 평면과 평행

            float t = Vector3.Dot(lb - ray.origin, normal) / denom;

            // 교차점 계산 (t < 0 경우도 허용 - 카메라가 평면을 넘어선 경우)
            Vector3 hitPoint = ray.origin + ray.direction * t;

            // 3. 교차점이 사각형 내부에 있는지 확인 (최적화된 UV 투영)
            // P = lb + u * edge1 + v * edge2 형태로 표현
            // u, v가 [0, 1] 범위 내에 있으면 사각형 내부
            Vector3 toHit = hitPoint - lb;

            // 수학적 최적화:
            // u = dot(toHit, edge1.normalized) / edge1.magnitude
            //   = dot(toHit, edge1 / |edge1|) / |edge1|
            //   = dot(toHit, edge1) / |edge1|²
            //   = dot(toHit, edge1) / edge1.sqrMagnitude
            // 제곱근 연산 제거! (sqrMagnitude는 단순 내적)

            float edge1SqrLen = edge1.sqrMagnitude;
            float edge2SqrLen = edge2.sqrMagnitude;

            float u = Vector3.Dot(toHit, edge1) / edge1SqrLen;
            float v = Vector3.Dot(toHit, edge2) / edge2SqrLen;

            // u, v가 [0, 1] 범위 내에 있으면 사각형 내부
            return (u >= 0f && u <= 1f && v >= 0f && v <= 1f);
        }
    }
}
