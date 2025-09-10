using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HisaCat.Structures
{
    public class ChildrenComponentsCache<T>
    {
        public readonly GameObject Target = null;

        public IReadOnlyList<T> Components => this.components;
        private T[] components = null;

        public ChildrenComponentsCache(GameObject target, bool cacheOnInitialize = true)
        {
            this.Target = target;
            if (cacheOnInitialize) CachingChildrenComponents();
        }

        public void CachingChildrenComponents()
        {
            var children = this.Target.GetComponentsInChildren<T>(true);
            this.components = children;

            PostCachingChildrenComponents();
        }
        protected virtual void PostCachingChildrenComponents() { }
    }

    public class ChildrenRenderersCache : ChildrenComponentsCache<Renderer>
    {
        public IReadOnlyList<bool> OriginalEnabledStates => this.originalEnabledStates;
        private bool[] originalEnabledStates = null;
        public IReadOnlyList<ShadowCastingMode> OriginalShadowCastingModes => this.originalShadowCastingModes;
        private ShadowCastingMode[] originalShadowCastingModes = null;
        public ChildrenRenderersCache(GameObject target, bool cacheOnInitialize = true) : base(target, cacheOnInitialize) { }

        protected override void PostCachingChildrenComponents()
        {
            var count = this.Components.Count;
            this.originalEnabledStates = new bool[count];
            this.originalShadowCastingModes = new ShadowCastingMode[count];
            for (int i = 0; i < count; i++)
            {
                var renderer = this.Components[i];
                this.originalEnabledStates[i] = renderer.enabled;
                this.originalShadowCastingModes[i] = renderer.shadowCastingMode;
            }
        }

        public void RestoreEnabledStates()
        {
            var count = this.Components.Count;
            for (int i = 0; i < count; i++)
                this.Components[i].enabled = this.originalEnabledStates[i];
        }
        public void RestoreShadowCastingModes()
        {
            var count = this.Components.Count;
            for (int i = 0; i < count; i++)
                this.Components[i].shadowCastingMode = this.originalShadowCastingModes[i];
        }

        public void SetEnabledStatesAll(bool enabled)
        {
            foreach (var renderer in this.Components)
                renderer.enabled = enabled;
        }
        public void SetForceRenderingOffAll(bool forceRenderingOff)
        {
            foreach (var renderer in this.Components)
                renderer.forceRenderingOff = forceRenderingOff;
        }
        public void SetShadowCastingModesAll(ShadowCastingMode mode)
        {
            foreach (var renderer in this.Components)
                renderer.shadowCastingMode = mode;
        }
    }

    public class ChildrenLightsCache : ChildrenComponentsCache<Light>
    {
        public IReadOnlyList<bool> OriginalEnabledStates => this.originalEnabledStates;
        private bool[] originalEnabledStates = null;
        public ChildrenLightsCache(GameObject target, bool cacheOnInitialize = true) : base(target, cacheOnInitialize) { }

        protected override void PostCachingChildrenComponents()
        {
            var count = this.Components.Count;
            this.originalEnabledStates = new bool[count];
            for (int i = 0; i < count; i++)
                this.originalEnabledStates[i] = this.Components[i].enabled;
        }

        public void RestoreEnabledStates()
        {
            var count = this.Components.Count;
            for (int i = 0; i < count; i++)
                this.Components[i].enabled = this.originalEnabledStates[i];
        }

        public void SetEnabledStatesAll(bool enabled)
        {
            foreach (var light in this.Components)
                light.enabled = enabled;
        }
    }
}
