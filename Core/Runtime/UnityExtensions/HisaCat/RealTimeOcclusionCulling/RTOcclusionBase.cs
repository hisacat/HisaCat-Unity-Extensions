using UnityEngine;
using System.Collections.Generic;
using System;

using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;

namespace HisaCat.RealTimeOcclusionCulling
{
    /// <summary>
    /// A base class for real-time occlusion components.
    /// </summary>
    [ExecuteAlways]
    public abstract class RTOcclusionBase : MonoBehaviour
    {
        protected abstract string ComponentName { get; }

        public Bounds Bounds => this.m_Bounds;
        [SerializeField] private Bounds m_Bounds = default;

        public IReadOnlyList<Renderer> Renderers => this.m_Renderers;
        public int RendererCount
        {
            get
            {
                EnsureRenderersSet();
                return this.m_Renderers.Count;
            }
        }
        public bool HasRenderers
        {
            get
            {
                EnsureRenderersSet();
                return this.m_Renderers.Count > 0;
            }
        }
        [SerializeField] private List<Renderer> m_Renderers = new();

        private HashSet<Renderer> renderersSet = null;
        private static readonly List<Renderer> rendererCleanupBuffer = new();

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;

            UpdateRenderersAutomatically();
            UpdateBoundsAutomatically();
#endif
        }
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
#endif
        }

        protected string FormatLog(string message) => $"[{ComponentName}] {message}";

        // Performance Optimization: (Profiler) To avoid call "Component.get_transform()" many times.
        public Transform TransformCache { get; private set; } = null;

        #region Transform Data Caches
        // World Bounds 캐싱 (성능 최적화)
        private bool isTransformDataCachesDirty = true;

        /// <summary>
        /// 캐싱된 로컬 좌표를 월드 좌표로 변환하는 매트릭스를 반환합니다. Transform이 변경되면 자동으로 갱신됩니다.
        /// </summary>
        public Matrix4x4 CachedLocalToWorldMatrix
        {
            get
            {
                if (isTransformDataCachesDirty) UpdateTransformDataCaches();
                return cachedLocalToWorldMatrix;
            }
        }
        private Matrix4x4 cachedLocalToWorldMatrix;

        /// <summary>
        /// 캐싱된 월드 공간 Bounds를 반환합니다. Transform이 변경되면 자동으로 갱신됩니다.
        /// </summary>
        public Bounds CachedWorldBounds
        {
            get
            {
                if (isTransformDataCachesDirty) UpdateTransformDataCaches();
                return cachedWorldBounds;
            }
        }
        private Bounds cachedWorldBounds;

        /// <summary>
        /// 캐싱된 월드 공간 8개 코너를 반환합니다. Transform이 변경되면 자동으로 갱신됩니다.
        /// </summary>
        public Vector3[] CachedWorldCorners
        {
            get
            {
                if (isTransformDataCachesDirty) UpdateTransformDataCaches();
                return cachedWorldCorners;
            }
        }
        private Vector3[] cachedWorldCorners = new Vector3[8];

        private void UpdateTransformDataCaches()
        {
            if (this.TransformCache == null)
                this.TransformCache = this.transform;

            this.cachedLocalToWorldMatrix = TransformCache.localToWorldMatrix;
            this.cachedWorldBounds = RTOcclusionUtility.CalculateWorldAABBFromCorners(cachedWorldCorners);
            RTOcclusionUtility.CalculateWorldCornersNonAlloc(this.Bounds, this.cachedLocalToWorldMatrix, this.cachedWorldCorners);
            this.isTransformDataCachesDirty = false;
        }
        #endregion Transform Data Caches

        protected virtual void Awake()
        {
            EnsureRenderersSet();

            this.TransformCache = this.transform;
        }
        protected virtual void OnEnable()
        {
            this.isTransformDataCachesDirty = true;
        }
        protected virtual void OnDisable() { }
        protected virtual void LateUpdate()
        {
            if (this.TransformCache.hasChanged)
            {
                this.OnTransformChanged_Internal();
                this.TransformCache.hasChanged = false;
            }
        }
        private void OnTransformChanged_Internal()
        {
            this.isTransformDataCachesDirty = true;
            this.OnTransformChanged();
        }
        protected virtual void OnTransformChanged() { }
        protected void InvalidateTransformCache() => this.isTransformDataCachesDirty = true;

        #region Bounds
        /// <summary>
        /// Calculates the combined bounds of the registered renderers in this object's local space.
        /// </summary>
        public Bounds CalculateRenderersBounds() => RTOcclusionUtility.CalculateRenderersCombinedLocalBounds(this.transform, this.m_Renderers);
        /// <summary>
        /// Automatically update bounds from registered renderers.
        /// </summary>
        public void UpdateBoundsAutomatically() => this.m_Bounds = CalculateRenderersBounds();
        #endregion Bounds

        #region Renderers
        public void ClearRenderers()
        {
            this.m_Renderers.Clear();
            if (this.renderersSet == null)
                this.renderersSet = new HashSet<Renderer>();
            else
                this.renderersSet.Clear();
        }
        public void UpdateRenderersAutomatically()
        {
            var renderers = this.GetComponentsInChildren<Renderer>(includeInactive: true);
            ClearRenderers();
            AddRenderers(renderers);
        }
        public void AddRenderers([NotNull] IEnumerable<Renderer> renderers, bool updateBounds = true)
        {
            if (renderers == null) throw new ArgumentNullException(nameof(renderers));

            EnsureRenderersSet();

            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    Debug.LogWarning(FormatLog($"{nameof(AddRenderers)}: Trying to add null renderer. It will be ignored."));
                    continue;
                }

                if (this.renderersSet.Add(renderer) == false)
                {
                    Debug.LogWarning(FormatLog($"{nameof(AddRenderers)}: Trying to add duplicated renderer '{renderer.name}'. It will be ignored."));
                    continue;
                }

                this.m_Renderers.Add(renderer);
                RendererAddedCallback(renderer);
            }

            if (updateBounds) UpdateBoundsAutomatically();
        }
        public bool AddRenderer([NotNull] Renderer renderer, bool updateBounds = true)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));

            EnsureRenderersSet();

            if (this.renderersSet.Add(renderer) == false) return false;

            this.m_Renderers.Add(renderer);

            RendererAddedCallback(renderer);

            if (updateBounds) UpdateBoundsAutomatically();

            return true;
        }
        protected virtual void RendererAddedCallback(Renderer renderer) { }

        public void RemoveRenderers([NotNull] IEnumerable<Renderer> renderers, bool updateBounds = true)
        {
            if (renderers == null) throw new ArgumentNullException(nameof(renderers));

            EnsureRenderersSet();

            foreach (var renderer in renderers)
                RemoveRendererInternal(renderer, logWarning: true);

            if (updateBounds) UpdateBoundsAutomatically();
        }
        public bool RemoveRenderer([NotNull] Renderer renderer, bool updateBounds = true)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));

            EnsureRenderersSet();

            bool removed = RemoveRendererInternal(renderer, logWarning: false);

            if (removed && updateBounds) UpdateBoundsAutomatically();

            return removed;
        }
        protected virtual void RendererRemovedCallback(Renderer renderer) { }
        #endregion Renderers

        #region Grids
        public HashSet<Vector3Int> OverlappedCells { get; private set; } = new();
        public void UpdateOverlappedCells(StaticBuffer<Vector3Int> cellsBuffer, int count)
        {
            this.OverlappedCells.Clear();
            for (int i = 0; i < count; i++)
                this.OverlappedCells.Add(cellsBuffer.Buffer[i]);
        }
        #endregion Grids

        #region Utility
        protected void ForEachRenderer(Action<Renderer> action, bool removeNullEntries = true, bool updateBoundsIfCleaned = false)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            EnsureRenderersSet();

            int removedNull = 0;
            for (int index = this.m_Renderers.Count - 1; index >= 0; index--)
            {
                var renderer = this.m_Renderers[index];
                if (renderer == null)
                {
                    if (removeNullEntries)
                    {
                        this.m_Renderers.RemoveAt(index);
                        removedNull++;
                    }
                    continue;
                }

                action(renderer);
            }

            if (removedNull > 0)
            {
                RemoveInvalidEntriesFromRendererSet();
                if (updateBoundsIfCleaned)
                    UpdateBoundsAutomatically();

                Debug.LogWarning(FormatLog($"{nameof(ForEachRenderer)}: Removed {removedNull} null renderer references during iteration."));
            }
        }

        protected int CleanupNullRenderers(bool updateBounds = true, bool log = true)
        {
            EnsureRenderersSet();

            int removed = this.m_Renderers.RemoveAll(static renderer => renderer == null);

            if (removed > 0)
            {
                RemoveInvalidEntriesFromRendererSet();

                if (updateBounds)
                    UpdateBoundsAutomatically();

                if (log)
                    Debug.LogWarning(FormatLog($"{nameof(CleanupNullRenderers)}: Removed {removed} null renderer references automatically."));
            }

            return removed;
        }

        protected IEnumerable<Renderer> EnumerateRenderers(bool skipNulls = true)
        {
            EnsureRenderersSet();

            for (int i = 0; i < this.m_Renderers.Count; i++)
            {
                var renderer = this.m_Renderers[i];
                if (renderer == null)
                {
                    if (skipNulls) continue;
                }

                yield return renderer;
            }
        }

        public bool TryGetRenderer(int index, out Renderer renderer)
        {
            EnsureRenderersSet();

            if (index < 0 || index >= this.m_Renderers.Count)
            {
                renderer = null;
                return false;
            }

            renderer = this.m_Renderers[index];
            if (renderer == null)
            {
                CleanupNullRenderers(updateBounds: false, log: false);
                return false;
            }

            return true;
        }

        private void RemoveInvalidEntriesFromRendererSet()
        {
            if (this.renderersSet == null || this.renderersSet.Count == 0) return;

            rendererCleanupBuffer.Clear();

            foreach (var renderer in this.renderersSet)
            {
                if (renderer == null)
                    rendererCleanupBuffer.Add(renderer);
            }

            if (rendererCleanupBuffer.Count == 0) return;

            for (int i = 0; i < rendererCleanupBuffer.Count; i++)
            {
                this.renderersSet.Remove(rendererCleanupBuffer[i]);
            }

            rendererCleanupBuffer.Clear();
        }

        private bool RemoveRendererInternal(Renderer renderer, bool logWarning)
        {
            if (renderer == null)
            {
                if (logWarning)
                    Debug.LogWarning(FormatLog($"{nameof(RemoveRendererInternal)}: Trying to remove null renderer. It will be ignored."));
                return false;
            }

            if (this.renderersSet.Contains(renderer) == false)
            {
                if (logWarning)
                    Debug.LogWarning(FormatLog($"{nameof(RemoveRendererInternal)}: Trying to remove non-registered renderer '{renderer.name}'. It will be ignored."));
                return false;
            }

            this.m_Renderers.Remove(renderer);
            this.renderersSet.Remove(renderer);

            RendererRemovedCallback(renderer);
            return true;
        }

        private void EnsureRenderersSet()
        {
            if (this.renderersSet != null) return;

            this.renderersSet = new HashSet<Renderer>();

            bool removedNullReference = false;
            bool removedDuplicateReference = false;

            for (int i = this.m_Renderers.Count - 1; i >= 0; i--)
            {
                var renderer = this.m_Renderers[i];
                if (renderer == null)
                {
                    this.m_Renderers.RemoveAt(i);
                    removedNullReference = true;
                    continue;
                }

                if (this.renderersSet.Add(renderer)) continue;

                this.m_Renderers.RemoveAt(i);
                removedDuplicateReference = true;
            }

            if (removedNullReference)
            {
                Debug.LogWarning(FormatLog($"{nameof(EnsureRenderersSet)}: Removed null renderer references from serialized data."));
            }

            if (removedDuplicateReference)
            {
                Debug.LogWarning(FormatLog($"{nameof(EnsureRenderersSet)}: Removed duplicated renderer references from serialized data."));
            }
        }
        #endregion Utility
    }
}
