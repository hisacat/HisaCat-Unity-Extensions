using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HisaCat.UI
{
    [RequireComponent(typeof(EventSystem))]
    public class SetPixelDragThresholdFromDPI : MonoBehaviour
    {
        private const float inchToCm = 2.54f;

        [SerializeField]
        private float dragThresholdCM = 0.5f;

        private void Awake()
        {
            var eventSystem = GetComponent<EventSystem>();
            eventSystem.pixelDragThreshold = (int)(dragThresholdCM * Screen.dpi / inchToCm);
        }
    }
}
