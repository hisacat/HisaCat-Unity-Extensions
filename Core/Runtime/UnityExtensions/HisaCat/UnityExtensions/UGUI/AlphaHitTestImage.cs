using HisaCat.HUE.UnityExtensions;
using HisaCat.PropertyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace HisaCat.HUE.UI
{
    /// <summary>
    /// This component is used to enable alpha hit test for the image.
    /// See https://docs.unity3d.com/2018.3/Documentation/ScriptReference/UI.Image-alphaHitTestMinimumThreshold.html
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class AlphaHitTestImage : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Image m_Image = null;
        [SerializeField, Range(0, 1)] private float m_AlphaHitTestMinimumThreshold = 1f / byte.MaxValue;
        public float AlphaHitTestMinimumThreshold
        {
            get => this.m_AlphaHitTestMinimumThreshold;
            set
            {
                this.m_AlphaHitTestMinimumThreshold = Mathf.Clamp(value, 0f, 1f);
                this.UpdateAlphaHitTestMinimumThreshold();
            }
        }

        private void Awake()
        {
            if (this.ConditionLogWarning(this.m_Image == null,
            $"[{nameof(AlphaHitTestImage)}] {nameof(Image)} is not assigned! it will be assigned automatically.", this))
                this.m_Image = GetComponent<Image>();

            this.UpdateAlphaHitTestMinimumThreshold();
        }

        private void UpdateAlphaHitTestMinimumThreshold()
            => this.m_Image.alphaHitTestMinimumThreshold = this.m_AlphaHitTestMinimumThreshold;

#if UNITY_EDITOR
        private void Reset()
        {
            if (this.m_Image == null)
                this.m_Image = GetComponent<Image>();

            this.UpdateAlphaHitTestMinimumThreshold();
        }
        private void OnValidate()
        {
            if (this.m_Image == null)
                this.m_Image = GetComponent<Image>();

            this.UpdateAlphaHitTestMinimumThreshold();
        }
#endif
    }
}
