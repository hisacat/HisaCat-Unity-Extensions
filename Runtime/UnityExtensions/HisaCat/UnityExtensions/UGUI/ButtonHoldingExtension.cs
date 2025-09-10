using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HisaCat.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonHoldingExtension : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region Events
        [System.Serializable]
        public class LongPressedEvent : UnityEvent<float> { }

        [FormerlySerializedAs("OnHolding")]
        [SerializeField]
        private LongPressedEvent m_OnHolding = new LongPressedEvent();
        public LongPressedEvent onHolding { get { return m_OnHolding; } set { m_OnHolding = value; } }
        #endregion

        private Button target = null;
        private void Awake()
        {
            this.target = GetComponent<Button>();
        }

        public float PointerDownStartedTime { get; private set; }
        public bool IsPointerDown { get; private set; }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            this.PointerDownStartedTime = Time.unscaledTime;
            this.IsPointerDown = true;
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            this.IsPointerDown = false;
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            this.IsPointerDown = false;
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) { }
        void IDragHandler.OnDrag(PointerEventData eventData) { }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) { }

        // For fix keep fire events from activated again from inactivated without pointer up/exit events.
        private void OnEnable()
        {
            this.IsPointerDown = false;
        }
        private void OnDisable()
        {
            this.IsPointerDown = false;
        }

        private void LateUpdate()
        {
            if (this.IsPointerDown)
            {
                if (this.target == null || this.target.interactable == false)
                {
                    this.IsPointerDown = false;
                    return;
                }

                var pointerDownDuration = Time.unscaledTime - this.PointerDownStartedTime;
                this.m_OnHolding.Invoke(pointerDownDuration);
            }
        }
    }
}
