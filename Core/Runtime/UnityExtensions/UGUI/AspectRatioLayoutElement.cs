using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HisaCat.UGUI
{
    /// <summary>
    /// Provides aspect-ratio-based preferred layout size for Unity UI layout groups.
    /// </summary>
    /// <remarks>
    /// This component does not directly modify the <see cref="RectTransform"/>.
    /// Instead, it reports <see cref="ILayoutElement.preferredWidth"/> or
    /// <see cref="ILayoutElement.preferredHeight"/> so that a parent layout group
    /// can size this element while preserving the desired aspect ratio.
    ///
    /// This is useful when a child of <see cref="HorizontalLayoutGroup"/> or
    /// <see cref="VerticalLayoutGroup"/> needs to keep an aspect ratio without
    /// fighting the parent layout group like <see cref="AspectRatioFitter"/> can.
    /// </remarks>
    [AddComponentMenu("HisaCat/UGUI/Layout/Aspect Ratio Layout Element")]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class AspectRatioLayoutElement : UIBehaviour, ILayoutElement
    {
        /// <summary>
        /// Determines which preferred size is calculated from the opposite axis.
        /// </summary>
        public enum AspectMode
        {
            /// <summary>
            /// Does not provide aspect-ratio-based preferred size.
            /// </summary>
            None,

            /// <summary>
            /// Calculates preferred height from the current width.
            /// </summary>
            /// <remarks>
            /// Formula: height = width / aspectRatio.
            /// </remarks>
            WidthControlsHeight,

            /// <summary>
            /// Calculates preferred width from the current height.
            /// </summary>
            /// <remarks>
            /// Formula: width = height * aspectRatio.
            /// </remarks>
            HeightControlsWidth,
        }

        /// <summary>
        /// Minimum allowed aspect ratio value.
        /// </summary>
        private const float MinAspectRatio = 0.001f;

        /// <summary>
        /// Maximum allowed aspect ratio value.
        /// </summary>
        private const float MaxAspectRatio = 1000f;

        [SerializeField]
        private AspectMode m_AspectMode = AspectMode.None;

        [SerializeField]
        [Min(MinAspectRatio)]
        private float m_AspectRatio = 1f;

        [SerializeField]
        private int m_LayoutPriority = 1;

        [System.NonSerialized]
        private RectTransform m_RectTransform;

        /// <summary>
        /// Cached <see cref="RectTransform"/> of this component.
        /// </summary>
        private RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                    m_RectTransform = GetComponent<RectTransform>();

                return m_RectTransform;
            }
        }

        /// <summary>
        /// Gets or sets how this element calculates preferred size from the aspect ratio.
        /// </summary>
        public AspectMode aspectMode
        {
            get => m_AspectMode;
            set
            {
                if (m_AspectMode == value)
                    return;

                m_AspectMode = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Gets or sets the target aspect ratio.
        /// </summary>
        /// <remarks>
        /// The aspect ratio is defined as width divided by height.
        /// For example, 16:9 should be set as <c>16f / 9f</c>.
        /// </remarks>
        public float aspectRatio
        {
            get => m_AspectRatio;
            set
            {
                value = Mathf.Clamp(value, MinAspectRatio, MaxAspectRatio);

                if (Mathf.Approximately(m_AspectRatio, value))
                    return;

                m_AspectRatio = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Gets or sets the layout priority used by Unity's layout system.
        /// </summary>
        /// <remarks>
        /// Higher priority layout elements are queried before lower priority ones
        /// when multiple <see cref="ILayoutElement"/> components exist on the same object.
        /// </remarks>
        public int priority
        {
            get => m_LayoutPriority;
            set
            {
                if (m_LayoutPriority == value)
                    return;

                m_LayoutPriority = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectRatioLayoutElement"/> class.
        /// </summary>
        protected AspectRatioLayoutElement() { }

#if UNITY_EDITOR
        /// <summary>
        /// Validates serialized values in the editor.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();

            m_AspectRatio = Mathf.Clamp(m_AspectRatio, MinAspectRatio, MaxAspectRatio);

            // Serialized field changes do not pass through property setters.
            SetDirty();
        }
#endif

        /// <summary>
        /// Marks this layout element dirty when it becomes active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        /// <summary>
        /// Marks this layout element dirty when it becomes inactive.
        /// </summary>
        protected override void OnDisable()
        {
            // Bypass IsActive() here because the layout must be rebuilt after this
            // element stops contributing preferred size.
            MarkLayoutForRebuild();

            base.OnDisable();
        }

        /// <summary>
        /// Marks the layout dirty before this element is moved to another parent.
        /// </summary>
        protected override void OnBeforeTransformParentChanged()
        {
            base.OnBeforeTransformParentChanged();

            // The old parent layout needs to forget this element's contribution.
            MarkLayoutForRebuild();
        }

        /// <summary>
        /// Marks the layout dirty after this element is moved to another parent.
        /// </summary>
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            // The new parent layout needs to include this element's contribution.
            SetDirty();
        }

        /// <summary>
        /// Marks the layout dirty when the RectTransform dimensions change.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            // The preferred size depends on the opposite axis, so dimension changes
            // can change the reported preferred size.
            SetDirty();
        }

        /// <summary>
        /// Marks the layout dirty when animation modifies serialized properties.
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            SetDirty();
        }

        /// <summary>
        /// Marks this layout element dirty if it is currently active.
        /// </summary>
        private void SetDirty()
        {
            if (!IsActive())
                return;

            MarkLayoutForRebuild();
        }

        /// <summary>
        /// Requests Unity's layout system to rebuild the layout containing this element.
        /// </summary>
        private void MarkLayoutForRebuild()
        {
            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();

            if (m_RectTransform == null)
                return;

            LayoutRebuilder.MarkLayoutForRebuild(m_RectTransform);
        }

        /// <summary>
        /// Calculates the preferred width reported to the layout system.
        /// </summary>
        /// <returns>
        /// The calculated preferred width, or <c>-1</c> when this component does not control width.
        /// </returns>
        protected virtual float CalculatePreferredWidth()
        {
            if (m_AspectMode == AspectMode.HeightControlsWidth)
                return rectTransform.rect.height * m_AspectRatio;

            return -1f;
        }

        /// <summary>
        /// Calculates the preferred height reported to the layout system.
        /// </summary>
        /// <returns>
        /// The calculated preferred height, or <c>-1</c> when this component does not control height.
        /// </returns>
        protected virtual float CalculatePreferredHeight()
        {
            if (m_AspectMode == AspectMode.WidthControlsHeight)
                return rectTransform.rect.width / m_AspectRatio;

            return -1f;
        }

        /// <inheritdoc />
        public virtual void CalculateLayoutInputHorizontal()
        {
            // Nothing is cached here because the value depends on the current RectTransform size.
        }

        /// <inheritdoc />
        public virtual void CalculateLayoutInputVertical()
        {
            // Nothing is cached here because the value depends on the current RectTransform size.
        }

        /// <inheritdoc />
        public float minWidth => -1f;

        /// <inheritdoc />
        public float minHeight => -1f;

        /// <inheritdoc />
        public float preferredWidth => CalculatePreferredWidth();

        /// <inheritdoc />
        public float preferredHeight => CalculatePreferredHeight();

        /// <inheritdoc />
        public float flexibleWidth => -1f;

        /// <inheritdoc />
        public float flexibleHeight => -1f;

        /// <inheritdoc />
        public int layoutPriority => m_LayoutPriority;
    }
}
