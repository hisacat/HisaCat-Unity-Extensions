using HisaCat.PropertyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HisaCat.UI
{
    [RequireComponent(typeof(EventSystem))]
    public class SetPixelDragThresholdFromDPI : MonoBehaviour
    {
        private const float InchToCm = 2.54f;
        [ReadOnly][SerializeField] private EventSystem eventSystem = null;
        [SerializeField] private float m_DragThresholdCm = 0.5f;

        public float DragThresholdCm
        {
            get => this.m_DragThresholdCm;
            set
            {
                this.m_DragThresholdCm = value;
                UpdatePixelDragThreshold();
            }
        }

        private void Awake()
        {
            UpdatePixelDragThreshold();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (this.eventSystem == null) this.eventSystem = GetComponent<EventSystem>();
            UpdatePixelDragThreshold();
        }
        private void OnValidate()
        {
            if (this.eventSystem == null) this.eventSystem = GetComponent<EventSystem>();
            UpdatePixelDragThreshold();
        }
#endif

        private void UpdatePixelDragThreshold()
        {
            Debug.Log(Screen.dpi);
            this.eventSystem.pixelDragThreshold = (int)(m_DragThresholdCm * Screen.dpi / InchToCm);
        }
    }
}
