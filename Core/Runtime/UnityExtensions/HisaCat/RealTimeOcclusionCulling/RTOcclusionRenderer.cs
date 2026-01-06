using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    [RequireComponent(typeof(Renderer))]
    public class RTOcclusionRenderer : MonoBehaviour
    {
        [SerializeField] private Renderer m_Renderer = null;
        protected virtual void Reset()
        {
#if UNITY_EDITOR
            this.m_Renderer = this.GetComponent<Renderer>();
#endif
        }

        private void OnEnable()
        {
            // this.SetCullStateFromGroup(true);
        }

        private CullingState cullingState = 0;
        public bool IsCulled => this.cullingState != CullingState.None;
        public bool IsCulledFromGroup => this.cullingState.HasFlag(CullingState.Group);
        public bool IsCulledFromOccluder => this.cullingState.HasFlag(CullingState.Occluder);

        public void SetCullStateFromGroup(bool cull)
        {
            this.cullingState |= CullingState.Group;
            UpdateCullingState();
        }
        public void SetCullStateFromOccluder(bool cull)
        {
            this.cullingState |= CullingState.Occluder;
            UpdateCullingState();
        }

        private void UpdateCullingState()
        {
            if (this.IsCulled != this.m_Renderer.forceRenderingOff)
                this.m_Renderer.forceRenderingOff = this.IsCulled;
        }
    }
}