using UnityEngine;
using System.Collections.Generic;
using System;

using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;

namespace HisaCat.RealTimeOcclusionCulling
{
    /// <summary>
    /// A base class for real-time occlusion components.
    /// </summary>
    public abstract class RTOcclusionBase : MonoBehaviour
    {
        protected abstract string ComponentName { get; }

        public Bounds Bounds => this.m_Bounds;
        [SerializeField] private Bounds m_Bounds = default;

        public IReadOnlyList<Renderer> Renderers => this.m_Renderers;
        [SerializeField] private List<Renderer> m_Renderers = new();

        private HashSet<Renderer> renderersSet = null;

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

        private static string FormatLog(string message) => $"[{nameof(ComponentName)}] {message}";
        protected virtual void Awake()
        {
            // Initialize renderersSet and validate renderers.
            this.renderersSet = new HashSet<Renderer>(this.m_Renderers);
            if (this.renderersSet.Contains(null)) this.renderersSet.Remove(null);

            if (this.renderersSet.Count != this.m_Renderers.Count)
            {
                int removeCount = this.m_Renderers.Count - this.renderersSet.Count;
                Debug.LogWarning(FormatLog($"{nameof(Awake)}: {removeCount} of duplicated or null renderer detected! they wll be removed."));

                this.m_Renderers = new List<Renderer>(this.renderersSet);
            }
        }

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
            this.renderersSet = new HashSet<Renderer>();
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

            using (var enumerator = renderers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var renderer = enumerator.Current;
                    if (renderer == null)
                    {
                        Debug.LogWarning(FormatLog($"{nameof(AddRenderers)}: Trying to add null renderer. It will be ignored."));
                        continue;
                    }

                    if (this.renderersSet.Contains(renderer))
                    {
                        Debug.LogWarning(FormatLog($"{nameof(AddRenderers)}: Trying to add duplicated renderer '{renderer.name}'. It will be ignored."));
                        continue;
                    }

                    this.m_Renderers.Add(renderer);
                    this.renderersSet.Add(renderer);

                    RendererAddedCallback(renderer);
                }
            }

            if (updateBounds) UpdateBoundsAutomatically();
        }
        public bool AddRenderer([NotNull] Renderer renderer, bool updateBounds = true)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (this.renderersSet.Contains(renderer)) return false;

            this.m_Renderers.Add(renderer);
            this.renderersSet.Add(renderer);

            RendererAddedCallback(renderer);

            if (updateBounds) UpdateBoundsAutomatically();

            return true;
        }
        protected virtual void RendererAddedCallback(Renderer renderer) { }

        public void RemoveRenderers([NotNull] IEnumerable<Renderer> renderers, bool updateBounds = true)
        {
            if (renderers == null) throw new ArgumentNullException(nameof(renderers));

            using (var enumerator = renderers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var renderer = enumerator.Current;
                    if (renderer == null)
                    {
                        Debug.LogWarning(FormatLog($"{nameof(RemoveRenderers)}: Trying to remove null renderer. It will be ignored."));
                        continue;
                    }

                    if (this.renderersSet.Contains(renderer) == false)
                    {
                        Debug.LogWarning(FormatLog($"{nameof(RemoveRenderers)}: Trying to remove non-registered renderer '{renderer.name}'. It will be ignored."));
                        continue;
                    }

                    this.m_Renderers.Remove(renderer);
                    this.renderersSet.Remove(renderer);

                    RendererRemovedCallback(renderer);
                }
            }

            if (updateBounds) UpdateBoundsAutomatically();
        }
        public bool RemoveRenderer([NotNull] Renderer renderer, bool updateBounds = true)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (this.renderersSet.Contains(renderer) == false) return false;

            this.m_Renderers.Remove(renderer);
            this.renderersSet.Remove(renderer);

            RendererRemovedCallback(renderer);

            if (updateBounds) UpdateBoundsAutomatically();

            return true;
        }
        protected virtual void RendererRemovedCallback(Renderer renderer) { }
        #endregion Renderers

        #region Grids
        public HashSet<Vector3Int> OverlappedCells { get; private set; } = new();
        public void UpdateOverlappedCells(Vector3Int[] cells, int count)
        {
            this.OverlappedCells.Clear();
            for (int i = 0; i < count; i++)
                this.OverlappedCells.Add(cells[i]);
        }
        #endregion Grids
    }
}
