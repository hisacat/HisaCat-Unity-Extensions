using FishNet;
using UnityEngine;

namespace HisaCat.FishNet.Components
{
    public class FishNetDebugToolsGUI : MonoBehaviour
    {
        private bool enableLatencySimulator = false;
        private float latency = 0f;

        private void Awake()
        {
            if (InstanceFinder.TransportManager != null)
            {
                if (InstanceFinder.TransportManager.LatencySimulator != null)
                {
                    this.enableLatencySimulator = InstanceFinder.TransportManager.LatencySimulator.GetEnabled();
                    this.latency = InstanceFinder.TransportManager.LatencySimulator.GetLatency();
                }
            }

            DontDestroyOnLoad(this.gameObject);
        }

        private void OnGUI()
        {
            float scale = Screen.height / 1080f;
            int fontSize = (int)(14 * scale);
            float padding = 10 * scale;

            float boxWidth = 300 * scale;
            float boxHeight = 100 * scale;
            float boxX = padding;
            float boxY = Screen.height - boxHeight - padding;

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                normal = { textColor = Color.white }
            };
            var toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = fontSize
            };
            var sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                fixedHeight = 10 * scale
            };
            var thumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                fixedWidth = 20 * scale,
                fixedHeight = 20 * scale
            };

            GUI.Box(new Rect(boxX, boxY, boxWidth, boxHeight), "");

#if !UNITY_EDITOR && !DEVELOPMENT
            GUI.Label(new Rect(boxX, boxY, boxWidth, boxHeight),
                "Latency Simulator Disabled\r\n\"DEVELOPMENT\" Define symbol required in build.",
                labelStyle);
#else
            Rect toggleRect = new Rect(boxX + padding, boxY + padding, boxWidth - 2 * padding, 20 * scale);
            this.enableLatencySimulator = GUI.Toggle(toggleRect, enableLatencySimulator, "Enable Latency Simulator", toggleStyle);

            Rect labelRect = new Rect(boxX + padding, boxY + padding + 25 * scale, boxWidth - 2 * padding, 20 * scale);
            GUI.Label(labelRect, $"Latency: {latency:0} ms", labelStyle);

            Rect sliderRect = new Rect(boxX + padding, boxY + padding + 50 * scale, boxWidth - 2 * padding, 20 * scale);
            this.latency = GUI.HorizontalSlider(sliderRect, latency, 0f, 2000f, sliderStyle, thumbStyle);

            if (GUI.changed)
            {
                if (InstanceFinder.TransportManager != null)
                {
                    if (InstanceFinder.TransportManager.LatencySimulator != null)
                    {
                        InstanceFinder.TransportManager.LatencySimulator.SetEnabled(this.enableLatencySimulator);
                        InstanceFinder.TransportManager.LatencySimulator.SetLatency((long)this.latency);
                    }
                }
            }
#endif
        }
    }
}
