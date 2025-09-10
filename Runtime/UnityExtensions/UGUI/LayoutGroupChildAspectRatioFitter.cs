using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HisaCat.UGUI
{
    [AddComponentMenu("HisaCat/UGUI/Layout/Layout Group Child Aspect Ratio Fitter")]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class LayoutGroupChildAspectRatioFitter : UIBehaviour, ILayoutElement
    {
        public enum AspectMode
        {
            None,
            WidthControlsHeight,
            HeightControlsWidth,
            // FitInParent,
            // EnvelopeParent
        }

        public AspectMode aspectMode { get => this.m_AspectMode; set => this.m_AspectMode = value; }
        [SerializeField] private AspectMode m_AspectMode = AspectMode.None;

        public float aspectRatio { get => this.m_AspectRatio; set => this.m_AspectRatio = value; }
        [SerializeField] private float m_AspectRatio = 1;

        [System.NonSerialized]
        private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null) m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            this.m_AspectRatio = Mathf.Clamp(this.m_AspectRatio, 0.001f, 1000f);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
#endif

        protected LayoutGroupChildAspectRatioFitter() { }

        protected override void OnEnable()
        {
            base.OnEnable();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }


        protected float CalculatePreferredWidth()
        {
            if (this.m_AspectMode == AspectMode.HeightControlsWidth)
                return this.rectTransform.rect.height * this.m_AspectRatio;
            return -1f;
        }
        protected float CalculatePreferredHeight()
        {
            if (this.m_AspectMode == AspectMode.WidthControlsHeight)
                return this.rectTransform.rect.width * this.m_AspectRatio;
            return -1f;
        }

        public virtual void CalculateLayoutInputHorizontal() { }
        public virtual void CalculateLayoutInputVertical() { }

        public float minWidth => -1;
        public float minHeight => -1;
        public float preferredWidth => CalculatePreferredWidth();
        public float preferredHeight => CalculatePreferredHeight();
        public float flexibleWidth => -1;
        public float flexibleHeight => -1;
        public int layoutPriority => 1;
    }
}
