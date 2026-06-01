using TMPro;
using UnityEngine;

namespace HisaCat.HUE
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class TextMeshProUGUILastCharacterFollower : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Target = null;
        [SerializeField] private Vector3 m_Offset = Vector3.zero;
        [SerializeField] private bool m_ForceMeshUpdate = false;
        private RectTransform rectTransform = null;
        private DrivenRectTransformTracker tracker;

        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            this.tracker.Clear();
            this.tracker.Add(this, this.rectTransform, DrivenTransformProperties.AnchoredPosition3D);
        }
        private void OnDisable()
        {
            this.tracker.Clear();
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (this.m_Target == null) return;
            if (this.rectTransform == null) return;

            if (this.m_ForceMeshUpdate) this.m_Target.ForceMeshUpdate();

            var textInfo = this.m_Target.textInfo;
            if (textInfo == null || textInfo.characterCount == 0)
            {
                this.rectTransform.anchoredPosition = Vector2.zero;
                return;
            }

            // Find last visible character
            int lastVisibleCharIndex = -1;
            for (int i = textInfo.characterCount - 1; i >= 0; i--)
            {
                if (textInfo.characterInfo[i].isVisible)
                {
                    lastVisibleCharIndex = i;
                    break;
                }
            }

            if (lastVisibleCharIndex < 0)
            {
                this.rectTransform.anchoredPosition = Vector2.zero;
                return;
            }

            var charInfo = textInfo.characterInfo[lastVisibleCharIndex];

            Vector3 charLocalPosition = (charInfo.topRight + charInfo.bottomRight) * 0.5f;
            Vector3 worldPosition = this.m_Target.transform.TransformPoint(charLocalPosition);
            Vector3 localPosition = this.transform.parent != null
                ? this.transform.parent.InverseTransformPoint(worldPosition)
                : worldPosition;

            this.transform.localPosition = localPosition + this.m_Offset;
        }
    }
}
