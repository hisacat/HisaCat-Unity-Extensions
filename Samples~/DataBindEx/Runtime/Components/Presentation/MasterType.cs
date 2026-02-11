namespace HisaCat.HUE.DataBindEx.Components.Presentation
{
    using Slash.Unity.DataBind.Core.Presentation;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Core/[DB] Master Type")]
    [DisallowMultipleComponent]
    public class MasterType : ContextHolder
    {
        private ContextHolder cachedParentContextHolder = null;
        protected override void Awake()
        {
            base.Awake();
            UpdateParentContextHolder();
        }
        protected virtual void OnTransformParentChanged()
        {
            UpdateParentContextHolder();
        }
        private void UpdateParentContextHolder()
        {
            if (cachedParentContextHolder != null)
            {
                cachedParentContextHolder.ContextChanged -= OnParentContextChanged;
                cachedParentContextHolder = null;
                OnParentContextChanged(null);
            }

            cachedParentContextHolder =
                transform.parent == null ? null : transform.parent.GetComponentInParent<ContextHolder>(includeInactive: true);
            if (cachedParentContextHolder != null)
            {
                cachedParentContextHolder.ContextChanged -= OnParentContextChanged;
                cachedParentContextHolder.ContextChanged += OnParentContextChanged;
                OnParentContextChanged(cachedParentContextHolder.Context);
            }
            else
            {
                OnParentContextChanged(null);
            }
        }
        private void OnParentContextChanged(object newContext)
        {
            this.Context = newContext;
        }
    }
}
