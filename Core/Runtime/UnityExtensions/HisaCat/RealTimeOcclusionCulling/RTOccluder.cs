using UnityEngine;
using System.Collections.Generic;
using HisaCat.UnityExtensions;

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
        // public bool EnableCullingSelf => this.m_EnableCullingSelf;
        // [SerializeField] private bool m_EnableCullingSelf = false;

        #region Face Portals Datas Cache
        private bool isFacePortalsDatasCacheDirty = true;

        private Dictionary<RTFacePortal, Bounds> facePortalsLocalBoundsCache = new();
        private Dictionary<RTFacePortal, Bounds> facePortalsWorldBoundsCache = new();
        private Dictionary<RTFacePortal, Vector3[]> facePortalsWorldCornersCache = new();
        public Bounds GetFacePortalLocalBoundsCache(RTFacePortal facePortal)
        {
            if (this.isFacePortalsDatasCacheDirty) UpdateFacePortalsDatasCache();
            CheckIsUnknownFacePortal(facePortal);

            return this.facePortalsLocalBoundsCache[facePortal];
        }
        public Bounds GetFacePortalWorldBoundsCache(RTFacePortal facePortal)
        {
            if (this.isFacePortalsDatasCacheDirty) UpdateFacePortalsDatasCache();
            CheckIsUnknownFacePortal(facePortal);

            return this.facePortalsWorldBoundsCache[facePortal];
        }
        public Vector3[] GetFacePortalWorldCornersCache(RTFacePortal facePortal)
        {
            if (this.isFacePortalsDatasCacheDirty) UpdateFacePortalsDatasCache();
            CheckIsUnknownFacePortal(facePortal);

            return this.facePortalsWorldCornersCache[facePortal];
        }
        private void CheckIsUnknownFacePortal(RTFacePortal facePortal)
        {
            if (this.facePortalsWorldCornersCache.ContainsKey(facePortal) == false)
            {
                throw new System.InvalidOperationException(
                    $"Face portal {facePortal.Face} not found in face portals world corners cache."
                    + $"\r\nIs it element of {nameof(this.m_FacePortals)} array?");
            }
        }
        private void UpdateFacePortalsDatasCache()
        {
            foreach (var facePortal in this.FacePortals)
            {
                // 1. Calculate local bounds
                if (this.facePortalsLocalBoundsCache.ContainsKey(facePortal) == false)
                    this.facePortalsLocalBoundsCache[facePortal] = default;
                this.facePortalsLocalBoundsCache[facePortal] = facePortal.GetLocalBounds(this);

                // 2. Calculate world corners
                if (this.facePortalsWorldCornersCache.ContainsKey(facePortal) == false)
                    this.facePortalsWorldCornersCache[facePortal] = new Vector3[8];
                RTOcclusionUtility.CalculateWorldCornersNonAlloc(
                    this.facePortalsLocalBoundsCache[facePortal],
                    this.CachedLocalToWorldMatrix,
                    this.facePortalsWorldCornersCache[facePortal]);

                // 3. Calculate world bounds
                if (this.facePortalsWorldBoundsCache.ContainsKey(facePortal) == false)
                    this.facePortalsWorldBoundsCache[facePortal] = default;
                this.facePortalsWorldBoundsCache[facePortal] = RTOcclusionUtility.CalculateWorldAABBFromCorners(this.facePortalsWorldCornersCache[facePortal]);
            }

            this.isFacePortalsDatasCacheDirty = false;
        }
        #endregion Face Portals Datas Cache

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (Application.isPlaying) return;
            this.isFacePortalsDatasCacheDirty = true;
        }
#endif

        override protected void OnEnable()
        {
            base.OnEnable();
            this.isFacePortalsDatasCacheDirty = true;
            RTOcclusionManager.RegisterOccluder(this);
        }
        override protected void OnDisable()
        {
            base.OnDisable();
            RTOcclusionManager.UnregisterOccluder(this);
        }

        public bool IsCulled { get; private set; }
        public void SetCulling(bool cull)
        {
            // if (this.ConditionLogError(this.m_EnableCullingSelf == false, "Culling self is disabled.")) return;

            if (this.IsCulled == cull) return;

            this.IsCulled = cull;

            for (int i = 0; i < this.Renderers.Count; i++)
            {
                var renderer = this.Renderers[i];
                if (renderer == null) continue;

                renderer.forceRenderingOff = cull;
            }
        }

        protected override void OnTransformChanged()
        {
            base.OnTransformChanged();
            RTOcclusionManager.OnOccluderTransformChanged(this);
        }
    }
}
