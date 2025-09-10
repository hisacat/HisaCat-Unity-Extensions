using UnityEngine;

namespace HisaCat.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaRectTransform : MonoBehaviour
    {
        private RectTransform rt = null;
        private Rect lastSafeArea = new Rect(0, 0, 0, 0);

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            ApplySafeArea(Screen.safeArea);

#if !UNITY_EDITOR
        Destroy(this);
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR
            Rect safeArea = Screen.safeArea;

            if (safeArea != lastSafeArea)
                ApplySafeArea(safeArea);
#endif
        }

        private void ApplySafeArea(Rect r)
        {
            lastSafeArea = r;

            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
        }
    }
}
