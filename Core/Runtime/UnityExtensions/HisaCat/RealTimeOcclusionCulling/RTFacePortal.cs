using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    /// <summary>
    /// Occluder의 어느 면에 Portal이 위치하는지 나타내는 열거형입니다.
    /// Unity 좌표계 기준으로 양수 방향의 3개 면을 나타냅니다.
    /// </summary>
    public enum RTBoundsFace
    {
        /// <summary>+X 면 (오른쪽 면)</summary>
        X,
        /// <summary>+Y 면 (위쪽 면)</summary>
        Y,
        /// <summary>+Z 면 (앞쪽 면)</summary>
        Z
    }

    /// <summary>
    /// Occluder의 한 면에 위치하는 사각형 Portal(창문, 문 등의 구멍)을 정의합니다.
    /// Portal은 Occluder의 로컬 공간에서 완전히 정의되며, Ray가 이 Portal을 통과하면
    /// Occluder 뒤의 오브젝트를 볼 수 있게 됩니다.
    /// </summary>
    /// <remarks>
    /// <para><b>좌표계 설명:</b></para>
    /// <para>
    /// <see cref="m_Center"/>와 <see cref="m_Size"/>는 <b>절대 로컬 공간 단위</b>로 지정됩니다.
    /// (정규화되거나 Occluder 크기에 상대적이지 않음)
    /// </para>
    /// <para><b>면별 축 매핑:</b></para>
    /// <list type="bullet">
    /// <item>
    /// <term>Face X (+X 면, 오른쪽)</term>
    /// <description>
    /// - Center.x → Z축 위치, Center.y → Y축 위치<br/>
    /// - Size.x → Z축 너비, Size.y → Y축 높이<br/>
    /// - Portal 평면: X축에 수직 (YZ 평면)
    /// </description>
    /// </item>
    /// <item>
    /// <term>Face Y (+Y 면, 위쪽)</term>
    /// <description>
    /// - Center.x → X축 위치, Center.y → Z축 위치<br/>
    /// - Size.x → X축 너비, Size.y → Z축 깊이<br/>
    /// - Portal 평면: Y축에 수직 (XZ 평면)
    /// </description>
    /// </item>
    /// <item>
    /// <term>Face Z (+Z 면, 앞쪽)</term>
    /// <description>
    /// - Center.x → X축 위치, Center.y → Y축 위치<br/>
    /// - Size.x → X축 너비, Size.y → Y축 높이<br/>
    /// - Portal 평면: Z축에 수직 (XY 평면)
    /// </description>
    /// </item>
    /// </list>
    /// <para><b>3D Bounds 계산:</b></para>
    /// <para>
    /// 계산된 3D Bounds는 Portal이 속한 면에 수직인 축을 따라 Occluder의 <b>전체 두께</b>를 차지합니다.
    /// 예: Z면 Portal의 경우, Portal의 2D 사각형은 XY 평면에 있고, Z축 방향으로는 Occluder 전체 두께만큼 확장됩니다.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public class RTFacePortal
    {
        /// <summary>
        /// Portal이 위치한 Occluder의 면 (X, Y, Z 중 하나의 양수 방향 면)
        /// </summary>
        [SerializeField] private RTBoundsFace m_Face = RTBoundsFace.Z;
        public RTBoundsFace Face => this.m_Face;

        /// <summary>
        /// Portal의 중심점 (면 평면상의 2D 좌표).<br/>
        /// Occluder의 로컬 공간 기준 절대 단위로 지정됩니다 (상대 좌표 아님).<br/>
        /// 예: (0, 0)은 Occluder 중심, (1, 0)은 Occluder 중심에서 해당 축으로 1 유닛 이동
        /// </summary>
        public Vector2 Center => this.m_Center;
        [SerializeField] private Vector2 m_Center = Vector2.zero;

        /// <summary>
        /// Portal의 크기 (면 평면상의 너비/높이).<br/>
        /// Occluder의 로컬 공간 기준 절대 단위로 지정됩니다 (비율 아님).<br/>
        /// 예: (2, 3)은 너비 2 유닛, 높이 3 유닛의 Portal
        /// </summary>
        public Vector2 Size => this.m_Size;
        [SerializeField] private Vector2 m_Size = Vector2.zero;

        /// <summary>
        /// Portal 활성화 여부. false일 경우 Portal이 존재하지 않는 것처럼 처리됩니다.
        /// </summary>
        public bool IsEnabled => this.m_IsEnabled;
        [SerializeField] private bool m_IsEnabled = true;

        /// <summary>
        /// Portal을 생성합니다.
        /// </summary>
        /// <param name="face">Portal이 위치할 Occluder의 면</param>
        /// <param name="center">면 평면상의 중심점 (절대 로컬 공간 단위)</param>
        /// <param name="size">면 평면상의 크기 (절대 로컬 공간 단위)</param>
        public RTFacePortal(RTBoundsFace face, Vector2 center, Vector2 size)
        {
            this.m_Face = face;
            this.m_Center = center;
            this.m_Size = size;
            this.m_IsEnabled = true;
        }

        /// <summary>
        /// 지정된 면에 크기가 0인 Portal을 생성합니다.
        /// </summary>
        /// <param name="face">Portal이 위치할 Occluder의 면</param>
        public RTFacePortal(RTBoundsFace face) : this(face, Vector2.zero, Vector2.zero) { }

        // TODO: Bounds cache

        /// <summary>
        /// Portal의 3D Bounds를 Occluder의 로컬 공간에서 계산합니다.<br/>
        /// Portal은 지정된 면에 정렬되며, 수직 축을 따라 Occluder의 전체 두께만큼 확장됩니다.
        /// </summary>
        /// <remarks>
        /// <para><b>동작 원리:</b></para>
        /// <para>
        /// - Portal의 2D 정보(Center, Size)를 해당 면의 3D 좌표계로 변환합니다.<br/>
        /// - 면에 수직인 축 방향으로는 Occluder의 전체 크기를 사용합니다.<br/>
        /// - 예: Z면 Portal의 경우, XY는 Portal 크기, Z는 Occluder 전체 두께
        /// </para>
        /// <para><b>사용처:</b></para>
        /// <para>
        /// 이 Bounds는 빠른 3D AABB 교차 검사에 사용됩니다.<br/>
        /// 더 정확한 검사가 필요한 경우 2D 평면 교차 검사를 사용합니다.
        /// </para>
        /// </remarks>
        /// <param name="container">Portal이 속한 Occluder</param>
        /// <returns>Occluder 로컬 공간의 Portal 3D Bounds</returns>
        public Bounds GetLocalBounds(RTOccluder container)
        {
            var occluderBounds = container.Bounds;
            var (center, size) = (occluderBounds.center, Vector3.zero);

            switch (this.m_Face)
            {
                // ============================================================
                // +X 면 (오른쪽 면): YZ 평면
                // ============================================================
                // Portal 평면: X축에 수직 (YZ 평면)
                // - Portal.Center.x → 월드 Z축 위치
                // - Portal.Center.y → 월드 Y축 위치
                // - Portal.Size.x → Z축 방향 너비
                // - Portal.Size.y → Y축 방향 높이
                // - X축 방향으로는 Occluder 전체 두께
                case RTBoundsFace.X:
                    center.x = occluderBounds.center.x;                    // X: Occluder 중심 (전체 두께)
                    center.y = occluderBounds.center.y + this.m_Center.y; // Y: Portal 중심 Y 좌표 적용
                    center.z = occluderBounds.center.z + this.m_Center.x; // Z: Portal 중심 X를 Z로 매핑
                    size.x = occluderBounds.size.x;                       // X: Occluder 전체 두께
                    size.y = this.m_Size.y;                               // Y: Portal 높이
                    size.z = this.m_Size.x;                               // Z: Portal 너비 (X를 Z로 매핑)
                    break;

                // ============================================================
                // +Y 면 (위쪽 면): XZ 평면
                // ============================================================
                // Portal 평면: Y축에 수직 (XZ 평면)
                // - Portal.Center.x → 월드 X축 위치
                // - Portal.Center.y → 월드 Z축 위치
                // - Portal.Size.x → X축 방향 너비
                // - Portal.Size.y → Z축 방향 깊이
                // - Y축 방향으로는 Occluder 전체 두께
                case RTBoundsFace.Y:
                    center.x = occluderBounds.center.x + this.m_Center.x; // X: Portal 중심 X 좌표 적용
                    center.y = occluderBounds.center.y;                    // Y: Occluder 중심 (전체 두께)
                    center.z = occluderBounds.center.z + this.m_Center.y; // Z: Portal 중심 Y를 Z로 매핑
                    size.x = this.m_Size.x;                               // X: Portal 너비
                    size.y = occluderBounds.size.y;                       // Y: Occluder 전체 두께
                    size.z = this.m_Size.y;                               // Z: Portal 깊이 (Y를 Z로 매핑)
                    break;

                // ============================================================
                // +Z 면 (앞쪽 면): XY 평면 (가장 일반적)
                // ============================================================
                // Portal 평면: Z축에 수직 (XY 평면)
                // - Portal.Center.x → 월드 X축 위치
                // - Portal.Center.y → 월드 Y축 위치
                // - Portal.Size.x → X축 방향 너비
                // - Portal.Size.y → Y축 방향 높이
                // - Z축 방향으로는 Occluder 전체 두께
                case RTBoundsFace.Z:
                default:
                    center.x = occluderBounds.center.x + this.m_Center.x; // X: Portal 중심 X 좌표 적용
                    center.y = occluderBounds.center.y + this.m_Center.y; // Y: Portal 중심 Y 좌표 적용
                    center.z = occluderBounds.center.z;                    // Z: Occluder 중심 (전체 두께)
                    size.x = this.m_Size.x;                               // X: Portal 너비
                    size.y = this.m_Size.y;                               // Y: Portal 높이
                    size.z = occluderBounds.size.z;                       // Z: Occluder 전체 두께
                    break;
            }

            return new Bounds(center, size);
        }
    }
}
