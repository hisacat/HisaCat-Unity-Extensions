using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    public enum RTBoundsFace
    {
        X,  // +X (Right)
        Y,  // +Y (Top)
        Z   // +Z (Front)
    }

    /// <summary>
    /// Describes a rectangular portal (a hole such as a window/door opening)
    /// that lies on one positive face of an <see cref="RTOccluder"/>'s local
    /// <see cref="Bounds"/>. The portal is defined entirely in the occluder's
    /// local space.
    /// </summary>
    /// <remarks>
    /// The <see cref="m_Center"/> and <see cref="m_Size"/> values are specified
    /// in ABSOLUTE local-space units (not normalized or relative to the occluder
    /// size). Axis mapping by face:<br/>
    /// - Face X (+X): Center (x→Z, y→Y), Size (x→Z width, y→Y height)<br/>
    /// - Face Y (+Y): Center (x→X, y→Z), Size (x→X width, y→Z height)<br/>
    /// - Face Z (+Z): Center (x→X, y→Y), Size (x→X width, y→Y height)<br/>
    /// The computed 3D bounds span the container's FULL thickness along the
    /// axis perpendicular to the face.
    /// </remarks>
    [System.Serializable]
    public class RTFacePortal
    {
        /// <summary>Positive face on which this portal lies.</summary>
        [SerializeField] private RTBoundsFace m_Face = RTBoundsFace.Z;
        /// <summary>
        /// Portal center on the face plane in occluder's local space, using
        /// absolute units (not relative).
        /// </summary>
        [SerializeField] private Vector2 m_Center = Vector2.zero;
        /// <summary>
        /// Portal size (width/height on the face plane) in occluder's local
        /// space, using absolute units (not relative).
        /// </summary>
        [SerializeField] private Vector2 m_Size = Vector2.zero;
        [SerializeField] private bool m_IsEnabled = true;

        /// <summary>Face of the occluder this portal belongs to.</summary>
        public RTBoundsFace Face => this.m_Face;
        /// <summary>Absolute local-space center on the face plane.</summary>
        public Vector2 Center => this.m_Center;
        /// <summary>Absolute local-space size on the face plane.</summary>
        public Vector2 Size => this.m_Size;
        /// <summary>Whether this portal is enabled.</summary>
        public bool IsEnabled => this.m_IsEnabled;

        public RTFacePortal(RTBoundsFace face, Vector2 center, Vector2 size)
        {
            this.m_Face = face;
            this.m_Center = center;
            this.m_Size = size;
            this.m_IsEnabled = true;
        }

        public RTFacePortal(RTBoundsFace face) : this(face, Vector2.zero, Vector2.zero) { }

        /// <summary>
        /// Returns the portal bounds in the occluder's local space, aligned
        /// to the specified face plane of <paramref name="container"/> and
        /// spanning the container's full thickness along the perpendicular axis.
        /// </summary>
        public Bounds GetBounds(RTOccluder container)
        {
            var occluderBounds = container.Bounds;
            var (center, size) = (occluderBounds.center, Vector3.zero);

            switch (this.m_Face)
            {
                case RTBoundsFace.X: // +X face (Right)
                    center.x = occluderBounds.center.x; // full thickness along X
                    center.y = occluderBounds.center.y + this.m_Center.y;
                    center.z = occluderBounds.center.z + this.m_Center.x;
                    size.x = occluderBounds.size.x;      // thickness
                    size.y = this.m_Size.y;              // height on face
                    size.z = this.m_Size.x;              // width on face
                    break;

                case RTBoundsFace.Y: // +Y face (Top)
                    center.y = occluderBounds.center.y; // full thickness along Y
                    center.x = occluderBounds.center.x + this.m_Center.x;
                    center.z = occluderBounds.center.z + this.m_Center.y;
                    size.y = occluderBounds.size.y;      // thickness
                    size.x = this.m_Size.x;              // width on face
                    size.z = this.m_Size.y;              // depth on face (mapped)
                    break;

                case RTBoundsFace.Z: // +Z face (Front)
                default:
                    center.z = occluderBounds.center.z; // full thickness along Z
                    center.x = occluderBounds.center.x + this.m_Center.x;
                    center.y = occluderBounds.center.y + this.m_Center.y;
                    size.z = occluderBounds.size.z;      // thickness
                    size.x = this.m_Size.x;              // width on face
                    size.y = this.m_Size.y;              // height on face
                    break;
            }

            return new Bounds(center, size);
        }
    }
}
