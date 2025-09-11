using UnityEngine;
using System.Collections.Generic;

namespace HisaCat.RealTimeOcclusionCulling
{
    /// <summary>
    /// A RTOccluder is a component that occludes the view of the camera.<br/>
    /// It can be used to cull the view of the camera from the objects in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class RTOccluder : RTOcclusionBase
    {
        protected override string ComponentName => nameof(RTOccluder);

        public IReadOnlyList<RTFacePortal> FacePortals => this.m_FacePortals;
        [SerializeField] private List<RTFacePortal> m_FacePortals = new();

        private void OnEnable() => RTOcclusionCamera.RegisterOccluder(this);
        private void OnDisable() => RTOcclusionCamera.UnregisterOccluder(this);
    }
}
