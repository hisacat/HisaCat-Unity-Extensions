using HisaCat.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class RTOcclusionCamera : MonoBehaviour
    {
        private void OnEnable() => RTOcclusionManager.RegisterOcclusionCamera(this);
        private void OnDisable() => RTOcclusionManager.UnregisterOcclusionCamera(this);

        private Camera _camera = null;
        public Camera Camera => this._camera.IsNotNull() ? this._camera : this._camera = this.GetComponent<Camera>();

        // Performance Optimization: (Profiler) To avoid call "Component.get_transform()" many times.
        public Transform TransformCache { get; private set; } = null;
        public Transform CameraTransformCache { get; private set; } = null;
        private void Awake()
        {
            this.TransformCache = this.transform;
            this.CameraTransformCache = this.Camera.transform;
        }

        private void OnDestroy() { }

        protected virtual void LateUpdate()
        {
            if (this.transform.hasChanged)
            {
                RTOcclusionManager.OnOcclusionCameraTransformChanged(this);
                this.transform.hasChanged = false;
            }
        }
    }
}
