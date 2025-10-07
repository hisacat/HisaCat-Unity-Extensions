namespace HisaCat.HUE.DataBindEx.Providers.Getters
{
    using Slash.Unity.DataBind.Foundation.Providers.Getters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Getters/[DB] RectTransform Rect Width Provider (Unity)")]
    public class RectTransformRectWidthProvider : ComponentDataProvider<RectTransform, float>
    {
        /// <inheritdoc />
        protected override void AddListener(RectTransform target)
        {
            var listener = target.GetComponent<RectTransformDimensionsChangeListener>();
            if (listener == null)
            {
                listener = target.gameObject.AddComponent<RectTransformDimensionsChangeListener>();
            }

            listener.DimensionsChanged += this.OnDimensionsChanged;
        }

        /// <inheritdoc />
        protected override float GetValue(RectTransform target)
        {
            return target.rect.width;
        }

        /// <inheritdoc />
        protected override void RemoveListener(RectTransform target)
        {
            var listener = target.GetComponent<RectTransformDimensionsChangeListener>();
            if (listener != null)
            {
                listener.DimensionsChanged -= this.OnDimensionsChanged;
            }
        }

        private void OnDimensionsChanged()
        {
            this.OnTargetValueChanged();
        }
    }
}
