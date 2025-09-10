using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    /// <summary>
    /// A RTOccludee is a component that is occluded by the RTOccluder.<br/>
    /// It can be used to cull the view of the camera from the objects in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class RTOccludee : RTOcclusionBase
    {
        protected override string ComponentName => nameof(RTOccludee);
        public bool IsCulled { get; private set; }

        private void OnEnable() => RTOcclusionCamera.RegisterOccludee(this);
        private void OnDisable() => RTOcclusionCamera.UnregisterOccludee(this);

        public void SetCulling(bool cull)
        {
            if (this.IsCulled == cull) return;

            this.IsCulled = cull;

            for (int i = 0; i < this.Renderers.Count; i++)
            {
                var renderer = this.Renderers[i];
                if (renderer == null) continue;

                renderer.forceRenderingOff = cull;
            }
        }

        protected override void RendererAddedCallback(Renderer renderer)
        {
            base.RendererAddedCallback(renderer);
            renderer.forceRenderingOff = this.IsCulled;
        }

        protected override void RendererRemovedCallback(Renderer renderer)
        {
            base.RendererRemovedCallback(renderer);
            renderer.forceRenderingOff = false;
        }
    }
}
