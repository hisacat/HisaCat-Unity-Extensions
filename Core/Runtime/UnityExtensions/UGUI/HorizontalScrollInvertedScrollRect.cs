using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HisaCat.UGUI
{
    /// <summary>
    /// Unity's <see cref="ScrollRect"/> has an inverted horizontal scroll direction.<br/>
    /// It fixes the horizontal scroll direction by inverting the X-axis scroll delta.
    /// </summary>
    public class HorizontalScrollInvertedScrollRect : ScrollRect
    {
        public override void OnScroll(PointerEventData data)
        {
            var originalScrollDelta = data.scrollDelta;

            data.scrollDelta = new Vector2(-originalScrollDelta.x, originalScrollDelta.y);
            base.OnScroll(data);

            data.scrollDelta = originalScrollDelta;
        }
    }
}
